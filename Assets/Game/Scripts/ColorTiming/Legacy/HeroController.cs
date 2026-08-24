using System;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Input.Adapters;
using ColorTiming.Player;
using ColorTiming.Presentation.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HeroController : MonoBehaviour, I_Damage, IGameInputConsumer, IGameplayPointerConsumer,
    IGameTimeConsumer, IPlayerDamageSignal
{
    public event Action Damaged;
    public float moveSpeed = 3.0f;
    public float dashSpeed = 5.0f;

   
    public GameObject characterSprite;
    public Animator animator;
    public WeaponControSystem weaponControSystem;
    public WeaponControSystem_2 WeaponControSystem_2;
    public GameObject deathShow;
    //HeroAnimStae heroAnimStae;


    public int heroMaxHP = 5;
    public int heroHP => vitality != null ? vitality.Health.Current : heroMaxHP;
    public UnityEvent OnSetHP_Event;
    public UnityEvent OnDamage_Event;   
    public UnityEvent OnPickUPWeapon;
    public UnityEvent<Weapon> OnSwitchWeapon;
    public UnityEvent<bool, AnimatorStateInfo> OnAnimState; 

    Rigidbody2D body;
    HeroSoundManager soundManager;

    Vector2 inputMove = Vector2.zero;

    Vector2 dashVector = Vector2.one;//冲刺方向

    //技能释放的冲刺
    Vector2 skillMoveVector = Vector2.zero ;
    float skillMoveSpeed = 26f * 2 / 3;
    //float skillMoveSpeed = 26f;    缩短至原来的三分之二 4-29

    public Weapon nowweapon { get; private set; }
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

    HeroFrireSystem heroFrireSystem;
    HeroAnimStae heroAnimState;
    PlayerActionStateMachine playerState;
    PlayerVitality vitality;
    PlayerWeaponInventory inventory;

    PlayerAttackInputGate attackInputGate;
    IGameInput gameInput;
    IGameplayPointerWorld pointerWorld;
    IGameTime gameTime;
    IDisposable hitSlowMotion;

    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    public void BindGameplayPointer(IGameplayPointerWorld pointer)
    {
        pointerWorld = pointer ?? throw new ArgumentNullException(nameof(pointer));
    }

    public void BindGameTime(IGameTime time)
    {
        gameTime = time ?? throw new ArgumentNullException(nameof(time));
    }
    
    void Start()
    {
        playerState = new PlayerActionStateMachine();
        attackInputGate = new PlayerAttackInputGate();
        vitality = new PlayerVitality(heroMaxHP);
        inventory = new PlayerWeaponInventory();
        OnDamage_Event?.Invoke();
        nowweapon = new Weapon(inventory.Current);
        body = GetComponent<Rigidbody2D>();

        heroAnimState = characterSprite.GetComponent<HeroAnimStae>();
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

        heroFrireSystem = GetComponent<HeroFrireSystem>();
        soundManager = GetComponentInChildren<HeroSoundManager>();
        //animator.GetCurrentAnimatorStateInfo
    }

    private void ONWudi(bool arg0)
    {
        playerState?.SetAnimationInvulnerable(arg0);
    }

    private void ONSkillMove(bool arg0)
    {
        playerState?.SetSkillMoving(arg0);
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
        playerState.Tick(Time.deltaTime);
        playerState.SetMove(inputMove.x, inputMove.y);

        if (gameInput.DashPressed)
        {
            if (nowAnimStateInfo.IsName("Daiji") || nowAnimStateInfo.IsName("Move"))
            {
                if (!playerState.IsDashing)
                {
                    //print("fuzhi de sha" + dashVector);
                    animator.SetTrigger(animPramNmae_Dash);
                }
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

    private void OnDashInvulnerability(bool active)
    {
        playerState?.SetDashInvulnerable(active);
    }

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
            if (nowweapon.weaponType == WeaponType.chuizhi || nowweapon.weaponType == WeaponType.futou)
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
            if (nowweapon.weaponType == WeaponType.chuizhi || nowweapon.weaponType == WeaponType.futou)
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
        playerState?.EndDash();
    }

    void Attack(string parm)
    {
        heroFrireSystem?.OnFire(nowweapon, characterSprite.transform.localScale.x,parm);
    }

    public bool PickUPWeapon(Weapon _w)
    {
        if (_w == null || inventory == null || !inventory.TryPickup(_w.Identity))
            return false;

        SwitchWeapon(new Weapon(inventory.Current));
        soundManager?.PlayAudio_Name("pickupWapon");

        return true;
    }
    void SwitchWeapon(Weapon weapon)
    {
        //print(weapon.GetIntType());

        nowweapon = weapon;
        animator.SetInteger(animPramName_WeaponType, nowweapon.GetIntType());

        animator.SetTrigger(animPramName_SwitchWeapon);
        OnSwitchWeapon?.Invoke(nowweapon);

    }
    //做一个丢弃武器功能  //接通BOSS 攻击
    void DisWeapon(bool dis)
    {
        if (inventory == null || !inventory.TryDrop(out var dropped))
        {
            if (!dis)
            {
                OnPickUPWeapon?.Invoke();
            }
           
            return;
        }

        soundManager?.PlayAudio_Name("disWeapon");
        var droppedWeapon = new Weapon(dropped);
        weaponControSystem?.CreateWeapon_dis(transform.position,droppedWeapon.colorType,droppedWeapon.weaponType);
        WeaponControSystem_2?.CreateWeapon_dis(transform.position, droppedWeapon.colorType, droppedWeapon.weaponType);

        SwitchWeapon(new Weapon(inventory.Current));
        
    }

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

    void EnterAnimState(AnimatorStateInfo stateInfo)
    {
        OnAnimState?.Invoke(true,stateInfo);
        nowAnimStateInfo = stateInfo;
        if (stateInfo.IsName(animPramNmae_Dash))
        {
            playerState.BeginDash();
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
            playerState.BeginAttack();
        }

        if (stateInfo.IsName("Daiji"))
        {
            playerState.SetSkillMoving(false);
            playerState.EndAttack();
        }
    }

    void ExitAnimState(AnimatorStateInfo stateInfo)
    {
        OnAnimState?.Invoke(false, stateInfo);
        //状态与参数同名
        if (stateInfo.IsName(animPramNmae_Dash))
        {
            playerState.EndDash();
        }

        if (stateInfo.IsName(animPramName_Atk))
        {
            playerState.EndAttack();
            if (inventory.ConsumeAttackWeapon(out _))
            {
                SwitchWeapon(new Weapon(inventory.Current));
            }
        }
        //这个方案延后了
    }

    public void OnDamage(GameObject _attacker, Weapon _weapon, Vector2 cPoint, string parm)
    {
        if (playerState == null || vitality == null || playerState.RejectsDamage)
        {
            //print("冲刺期间无敌");
            return;
        }

        if (playerState.IsDashing)
        {
            gameTime?.Pulse(0.45f, 0.3f);
            //加一个闪避成功回一滴血
            if (vitality.ResolveSuccessfulDash() > 0)
            {
                OnSetHP_Event?.Invoke();
            }
            return;
        }

        //parm.Contains()
        bool ms = parm != null && parm.Contains("miaosha");
        var resolution = vitality.TakeDamage(1, false, ms);

        if (resolution == PlayerDamageResolution.Damaged)
        {
            OnSetHP_Event?.Invoke();
            OnDamage_Event?.Invoke();
            Damaged?.Invoke();
            
            if (cPoint == Vector2.zero) print("计算受击时，获取到一个 0 位置");
            Vector2 cv = (new Vector2(transform.position.x, transform.position.y) - cPoint).normalized;

            //float force = 10;
            Vector2 fc = cv * 0.5f;
            //rigidbody2D.AddForce(cv * force);
            Vector3 af = new Vector3(fc.x,fc.y,0);
            transform.position += af;
            DisWeapon(true);
            animator.SetTrigger(animPramName_Hit);
            //这个过程中不可以移动
        }
        else if (resolution == PlayerDamageResolution.Defeated)
        {
            hitSlowMotion?.Dispose();
            hitSlowMotion = null;
            playerState.Kill();
            //print("角色战败，失败界面未接");
            //显示失败，并准备退回开始页面
            SpriteRenderer sprite = animator.GetComponent<SpriteRenderer>();
            //m_SortingOrder  m_SortingLayerID
            sprite.sortingOrder = 200;

            HeroCamera_ heroCamera_ = GetComponent<HeroCamera_>();
            heroCamera_.enabled = false;

            animator.SetTrigger("Death");
            deathShow?.SetActive(true);
        } 
    }

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
            playerState.BeginHit();
        }
        else
        {
            playerState.EndHit();
        }
    }

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

