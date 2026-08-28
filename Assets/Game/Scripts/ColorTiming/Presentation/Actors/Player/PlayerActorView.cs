// 文件职责：负责 玩家Actor 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using System;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Player;
using ColorTiming.Presentation.Combat;
using ColorTiming.Presentation.UI.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerActorView : MonoBehaviour, IBattleDamageReceiver, IGameInputConsumer, IGameplayPointerConsumer,
    IGameTimeConsumer, IPlayerDamageSignal
{
    public event Action Damaged;
    public ActorId DamageActorId => ActorId.Player;
    public float moveSpeed = 3.0f;
    public float dashSpeed = 5.0f;


    public GameObject characterSprite;
    public Animator animator;
    [FormerlySerializedAs("weaponControSystem")]
    public Boss1WeaponSpawnerView boss1WeaponSpawner;
    [FormerlySerializedAs("WeaponControSystem_2")]
    public Boss2WeaponSpawnerView boss2WeaponSpawner;
    public GameObject deathShow;


    public int heroMaxHP = 5;
    public int heroHP => battleSession != null ? battleSession.Snapshot.PlayerHealth : heroMaxHP;
    public UnityEvent OnSetHP_Event;
    public UnityEvent OnDamage_Event;
    public UnityEvent OnPickUPWeapon;
    public UnityEvent<WeaponIdentity> OnSwitchWeapon;
    public UnityEvent<bool, AnimatorStateInfo> OnAnimState;

    Rigidbody2D body;
    PlayerSoundView soundManager;

    Vector2 inputMove = Vector2.zero;

    Vector2 dashVector = Vector2.one;//冲刺方向

    //技能释放的冲刺
    Vector2 skillMoveVector = Vector2.zero ;
    float skillMoveSpeed = 26f * 2 / 3;
    //float skillMoveSpeed = 26f;    缩短至原来的三分之二 4-29

    public WeaponIdentity nowweapon { get; private set; }
    AnimatorStateInfo nowAnimStateInfo;
    //动画器跳转事件，1.状态名称 2.true=进入，false=退出
    public UnityEvent<AnimatorStateInfo, bool> OnAnimStateEnter;
    const string animPramName_MoveSpeed = "moveSpeed";
    const string animPramName_MoveV= "moveV";
    const string animPramNmae_Dash = "Dash";
    const string animPramName_Atk = "Atk";
    const string animPramName_Atk_x = "Atk_x";
    const string animPramName_WeaponType = "weaponType";
    const string animPramName_SwitchWeapon = "switchWeapon";
    const string animPramName_Hit = "Hit";

    PlayerSkillEmitter heroFrireSystem;
    PlayerAnimationEventRelay heroAnimState;
    PlayerActionStateMachine playerState;
    PlayerWeaponInventory inventory;
    BattleSession battleSession;
    bool viewStarted;
    bool sessionInitialized;

    PlayerAttackInputGate attackInputGate;
    IGameInput gameInput;
    IGameplayPointerWorld pointerWorld;
    IGameTime gameTime;
    IDisposable hitSlowMotion;

    /// <summary>Bound by the runtime-created battle composition root before Start.</summary>
    // 绑定战斗会话依赖或事件监听。
    public void BindBattleSession(BattleSession session)
    {
        if (battleSession != null && !ReferenceEquals(battleSession, session))
        {
            throw new InvalidOperationException("Hero is already bound to another battle session.");
        }
        battleSession = session ?? throw new ArgumentNullException(nameof(session));
        playerState = session.PlayerActions;
        inventory = session.Inventory;
        TryInitializeSession();
    }

    // 绑定Game输入依赖或事件监听。
    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    // 绑定Gameplay指针依赖或事件监听。
    public void BindGameplayPointer(IGameplayPointerWorld pointer)
    {
        pointerWorld = pointer ?? throw new ArgumentNullException(nameof(pointer));
    }

    // 绑定游戏时间依赖或事件监听。
    public void BindGameTime(IGameTime time)
    {
        gameTime = time ?? throw new ArgumentNullException(nameof(time));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        attackInputGate = new PlayerAttackInputGate();
        body = GetComponent<Rigidbody2D>();

        heroAnimState = characterSprite.GetComponent<PlayerAnimationEventRelay>();
        if (heroAnimState != null)
        {
            heroAnimState.OnDashEnd.AddListener(DashEnd);
            heroAnimState.OnDashWD.AddListener(OnDashInvulnerability);
            heroAnimState.OnAttack.AddListener(Attack);
            heroAnimState.OnHit.AddListener(OnHit_Anim);
            heroAnimState.OnSkillMove.AddListener(ONSkillMove);
            heroAnimState.OnWudi.AddListener(ONWudi);
            //已放弃使用该方案
            //OnDamage_Event.AddListener(heroAnimStae.ShowHitColor);
            OnSwitchWeapon.AddListener(heroAnimState.ShowWeaponColor);
        }

        OnAnimStateEnter.AddListener(OnAnimStateEnterF);

        heroFrireSystem = GetComponent<PlayerSkillEmitter>();
        soundManager = GetComponentInChildren<PlayerSoundView>();
        viewStarted = true;
        TryInitializeSession();
        //animator.GetCurrentAnimatorStateInfo
    }

    // 尝试Initialize会话，并通过返回值报告是否成功。
    void TryInitializeSession()
    {
        if (sessionInitialized || !viewStarted || battleSession == null) return;
        sessionInitialized = true;
        nowweapon = inventory.Current;
        OnDamage_Event?.Invoke();
        OnSetHP_Event?.Invoke();
        OnSwitchWeapon?.Invoke(nowweapon);
    }

    private void ONWudi(bool arg0)
    {
        battleSession?.SetAnimationInvulnerable(arg0);
    }

    private void ONSkillMove(bool arg0)
    {
        battleSession?.SetSkillMoving(arg0);
        //skillMoveVector = inputMove;
        if (gameInput == null || pointerWorld == null)
        {
            return;
        }

        //修改为朝向指针方向
        Vector2 world = pointerWorld.Resolve(gameInput.PointerScreenPosition);
        Vector3 cv = new Vector3(world.x, world.y, transform.position.z) - transform.position;

        skillMoveVector = cv.normalized;
        inputMove = skillMoveVector;
        Flip();
        //使玩家转向
    }



    // Update is called once per frame
    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {
        if (playerState == null || !playerState.IsAlive || !(Time.timeScale > 0))
        {
            attackInputGate?.Reset();
            return;
        }

        if (gameInput == null)
        {
            return;
        }

        attackInputGate.Tick(Time.deltaTime);

        inputMove = gameInput.Move;
        battleSession.Tick(Time.deltaTime);
        battleSession.SetMove(inputMove.x, inputMove.y);

        var animatorState = animator.GetCurrentAnimatorStateInfo(0);
        if (playerState.IsHitStunned
            && (animatorState.IsName("Daiji") || animatorState.IsName("Move")))
        {
            // The authored Hit end event is the primary signal, while locomotion is a
            // deterministic recovery boundary if Animator exits Hit without delivering it.
            battleSession.SetHitAnimationActive(false);
        }

        if (gameInput.DashPressed)
        {
            if (playerState.CanMove && battleSession.TryBeginDash())
            {
                // Accept the intent in the authoritative session before asking Animator to
                // present it. Animator callbacks remain a fallback for authored transitions.
                dashVector = new Vector2(playerState.FacingX, playerState.DashY);
                animator.SetTrigger(animPramNmae_Dash);
            }
        }


        //为了解决暂停恢复通过攻击键
        if (attackInputGate.IsReady)
        {
            if (attackInputGate.ShouldTrigger(gameInput.AttackPressed, playerState.IsAttacking))
            {
                animator.SetTrigger(animPramName_Atk);
            }

            animator.SetFloat(
                animPramName_Atk_x,
                attackInputGate.HeldAnimatorValue(gameInput.AttackHeld));
        }



        if (gameInput.DropPressed)
        {
            DisWeapon(false);
        }

    }

    // 响应冲刺Invulnerability回调，并更新本对象状态。
    private void OnDashInvulnerability(bool active)
    {
        battleSession?.SetDashInvulnerable(active);
    }

    // 按物理帧推进与刚体或碰撞相关的状态。
    private void FixedUpdate()
    {
        if (playerState == null || playerState.IsHitStunned || !playerState.IsAlive) return;

        Move();
        Flip();
        Dashing();
        SkillMove();
    }

    void Flip()
    {
        //print(inputMove);
        if (!Mathf.Approximately(inputMove.x, 0) && !playerState.IsDashing && !playerState.IsAttacking && !playerState.IsSkillMoving)
        {
            bool isMoveLeft = inputMove.x < 0;
            //float look = isMoveLeft ? 180 : 0;
            float look = isMoveLeft ? -1 : 1;
            //characterSprite.transform.Rotate(new Vector3(0, look, 0));
            characterSprite.transform.localScale = new Vector3(look, 1, 1);
            //characterSprite.transform.rotation.SetLookRotation(new Vector3(0,look,0));
            dashVector = new Vector2(look, inputMove.y);
        }

    }

    void Move()
    {
        animator.SetFloat(animPramName_MoveSpeed, inputMove.magnitude * 10);
        if (!playerState.CanMove)
            return;//冲刺中不允移动


        Vector2 _move = Vector2.zero;
        if (!Mathf.Approximately(inputMove.x, 0) || (!Mathf.Approximately(inputMove.y, 0)))
        {
            if (nowweapon.Type == ColorTiming.Combat.WeaponType.Hammer
                || nowweapon.Type == ColorTiming.Combat.WeaponType.Axe)
            {
                animator.SetLayerWeight(1, 1);
                animator.SetLayerWeight(0, 0);
            }

            if (inputMove.magnitude > 0.1f)
            {
                _move = inputMove.normalized;

                _move = _move * moveSpeed * Time.deltaTime;

                Vector2 go = transform.position + new Vector3(_move.x, _move.y, 0);
                body.MovePosition(go);
            }


        }
        else
        {
            if (nowweapon.Type == ColorTiming.Combat.WeaponType.Hammer
                || nowweapon.Type == ColorTiming.Combat.WeaponType.Axe)
            {
                animator.SetLayerWeight(1, 0);
                animator.SetLayerWeight(0, 1);
            }

        }


        //print(characterController.velocity.magnitude); //这个值没有停止
        //print(inputMove.magnitude);
        //print(_move.magnitude);
        animator.SetFloat(animPramName_MoveSpeed, _move.magnitude * 10);


    }

    void Dashing()
    {
        if (playerState.CanEvadeDamage)
        {
            Vector2 _dashMove = new Vector2(playerState.FacingX, playerState.DashY);
            //始终为横向赋值
            _dashMove.x = _dashMove.x > 0 ? 1 : -1;
            //检查纵向是否有值
            if (_dashMove.y != 0) _dashMove.y = _dashMove.y > 0 ? 0.5f : -0.5f;
            //Vector2 _dashMove =
            _dashMove = _dashMove * dashSpeed * Time.deltaTime;
            _dashMove = transform.position + new Vector3(_dashMove.x, _dashMove.y);

            body.MovePosition(_dashMove);

            //animator.ResetTrigger(animPramNmae_Dash);
            //如何让其在正确的时机进行关闭？
        }
    }
    void SkillMove()
    {
        if (playerState.IsSkillMoving)
        {

            //print(dashVector);
            Vector2 _move = skillMoveVector == Vector2.zero?dashVector:skillMoveVector;

            if(_move.x != 0) _move.x = _move.x > 0 ? 1 : -1;
            if (_move.y != 0) _move.y = _move.y>0 ? 1 : -1;

            //始终为横向赋值
            //_move.x = _move.x > 0 ? 1 : -1;
            //检查纵向是否有值
            //if (_move.y != 0) _move.y = _move.y > 0 ? 0.5f : -0.5f;
            //Vector2 _dashMove =
            _move.Normalize();
            _move = _move * skillMoveSpeed * Time.deltaTime;
            _move = transform.position + new Vector3(_move.x, _move.y);

            body.MovePosition(_move);
        }
    }

    void DashEnd()
    {
        //print("Dash End!!");
        battleSession?.EndDash();
    }

    void Attack(string parm)
    {
        heroFrireSystem?.OnFire(nowweapon, characterSprite.transform.localScale.x,parm);
    }

    // 执行PickUP武器对应的主要流程。
    public bool PickUPWeapon(WeaponIdentity weapon)
    {
        if (battleSession == null || !battleSession.TryPickup(weapon))
            return false;

        SwitchWeapon(inventory.Current);
        soundManager?.Play(PlayerSoundCue.PickupWeapon);

        return true;
    }
    void SwitchWeapon(WeaponIdentity weapon)
    {
        //print(weapon.GetIntType());

        nowweapon = weapon;
        animator.SetInteger(animPramName_WeaponType, nowweapon.ToLegacyAnimatorIndex());

        animator.SetTrigger(animPramName_SwitchWeapon);
        OnSwitchWeapon?.Invoke(nowweapon);

    }
    //做一个丢弃武器功能  //接通BOSS 攻击
    void DisWeapon(bool dis)
    {
        if (battleSession == null || !battleSession.TryDrop(out var dropped))
        {
            if (!dis)
            {
                OnPickUPWeapon?.Invoke();
            }

            return;
        }

        soundManager?.Play(PlayerSoundCue.DropWeapon);
        boss1WeaponSpawner?.CreateWeapon_dis(transform.position, dropped);
        boss2WeaponSpawner?.CreateWeapon_dis(transform.position, dropped);

        SwitchWeapon(inventory.Current);

    }

    /// <summary>
    /// 接收 Animator StateMachineBehaviour 的状态切换通知。
    /// </summary>
    internal void NotifyAnimationState(AnimatorStateInfo stateInfo, bool entered)
    {
        OnAnimStateEnter?.Invoke(stateInfo, entered);
    }

    // 响应Anim状态EnterF回调，并更新本对象状态。
    private void OnAnimStateEnterF(AnimatorStateInfo stateInfo, bool enter)
    {
        if (enter)
        {
            EnterAnimState(stateInfo);
        }
        else
        {
            ExitAnimState(stateInfo);
        }
    }

    // 执行EnterAnim状态对应的主要流程。
    void EnterAnimState(AnimatorStateInfo stateInfo)
    {
        OnAnimState?.Invoke(true,stateInfo);
        nowAnimStateInfo = stateInfo;
        if (stateInfo.IsName(animPramNmae_Dash))
        {
            if (!playerState.IsDashing)
            {
                battleSession?.TryBeginDash();
            }
            dashVector = new Vector2(playerState.FacingX, playerState.DashY);
        }
        if (stateInfo.IsName("switchWeapon"))
        {
            animator.ResetTrigger(animPramName_Atk);
            animator.ResetTrigger(animPramNmae_Dash);
            animator.ResetTrigger(animPramName_SwitchWeapon);
            animator.SetFloat(animPramName_Atk_x,0);
        }

        if (stateInfo.IsName(animPramName_Atk))
        {
            battleSession?.TryBeginAttack();
        }

        if (stateInfo.IsName("Daiji"))
        {
            battleSession?.SetSkillMoving(false);
            battleSession?.EndAttack();
        }
    }

    // 执行ExitAnim状态对应的主要流程。
    void ExitAnimState(AnimatorStateInfo stateInfo)
    {
        OnAnimState?.Invoke(false, stateInfo);
        //状态与参数同名
        if (stateInfo.IsName(animPramNmae_Dash))
        {
            battleSession?.EndDash();
        }

        if (stateInfo.IsName(animPramName_Atk))
        {
            battleSession?.EndAttack();
            if (battleSession.ConsumeAttackWeapon(out _))
            {
                SwitchWeapon(inventory.Current);
            }
        }
        //这个方案延后了
    }

    public void ReceiveDamage(BattleDamage damage)
    {
        if (playerState == null || battleSession == null || playerState.RejectsDamage)
        {
            //print("冲刺期间无敌");
            return;
        }

        if (playerState.CanEvadeDamage)
        {
            gameTime?.Pulse(0.45f, 0.3f);
            //加一个闪避成功回一滴血
            if (battleSession.ResolveSuccessfulDash() > 0)
            {
                OnSetHP_Event?.Invoke();
            }
            return;
        }

        //parm.Contains()
        var heldWeapon = inventory.Current;
        var resolution = battleSession.ApplyPlayerDamage(damage);

        if (resolution == PlayerDamageResolution.Damaged)
        {
            OnSetHP_Event?.Invoke();
            OnDamage_Event?.Invoke();
            Damaged?.Invoke();

            var cPoint = new Vector2(damage.ContactPoint.X, damage.ContactPoint.Y);
            if (cPoint == Vector2.zero) print("计算受击时，获取到一个 0 位置");
            Vector2 cv = (new Vector2(transform.position.x, transform.position.y) - cPoint).normalized;

            //float force = 10;
            Vector2 fc = cv * 0.5f;
            //rigidbody2D.AddForce(cv * force);
            Vector3 af = new Vector3(fc.x,fc.y,0);
            transform.position += af;
            if (!heldWeapon.IsNormal)
            {
                soundManager?.Play(PlayerSoundCue.DropWeapon);
                boss1WeaponSpawner?.CreateWeapon_dis(transform.position, heldWeapon);
                boss2WeaponSpawner?.CreateWeapon_dis(transform.position, heldWeapon);
                SwitchWeapon(inventory.Current);
            }
            animator.SetTrigger(animPramName_Hit);
            //这个过程中不可以移动
        }
        else if (resolution == PlayerDamageResolution.Defeated)
        {
            hitSlowMotion?.Dispose();
            hitSlowMotion = null;
            //print("角色战败，失败界面未接");
            //显示失败，并准备退回开始页面
            SpriteRenderer sprite = animator.GetComponent<SpriteRenderer>();
            //m_SortingOrder  m_SortingLayerID
            sprite.sortingOrder = 200;

            PlayerCameraLifecycleView heroCamera_ = GetComponent<PlayerCameraLifecycleView>();
            heroCamera_.enabled = false;

            animator.SetTrigger("Death");
            deathShow?.SetActive(true);
        }
    }

    // 响应HitAnim回调，并更新本对象状态。
    private void OnHit_Anim(int arg0)
    {
        if (playerState == null || !playerState.IsAlive) return;

        if (arg0 == 0)
        {
            //角色动画已忽略时间
            //暂停世界
            animator.speed = 10;
            hitSlowMotion?.Dispose();
            hitSlowMotion = gameTime?.Acquire(0.1f);

        }
        else
        {
            animator.speed = 1;
            hitSlowMotion?.Dispose();
            hitSlowMotion = null;
        }

        if (arg0 < 2)
        {
            battleSession?.SetHitAnimationActive(true);
        }
        else
        {
            battleSession?.SetHitAnimationActive(false);
        }
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        OnAnimStateEnter.RemoveListener(OnAnimStateEnterF);
        if (heroAnimState != null)
        {
            heroAnimState.OnDashEnd.RemoveListener(DashEnd);
            heroAnimState.OnDashWD.RemoveListener(OnDashInvulnerability);
            heroAnimState.OnAttack.RemoveListener(Attack);
            heroAnimState.OnHit.RemoveListener(OnHit_Anim);
            heroAnimState.OnSkillMove.RemoveListener(ONSkillMove);
            heroAnimState.OnWudi.RemoveListener(ONWudi);
            OnSwitchWeapon.RemoveListener(heroAnimState.ShowWeaponColor);
        }
        hitSlowMotion?.Dispose();
        hitSlowMotion = null;
    }
}
