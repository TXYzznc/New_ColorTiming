// 文件职责：负责 玩家Actor 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using System;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using ColorTiming.Input;
using ColorTiming.Player;
using ColorTiming.Presentation.Combat;
using ColorTiming.Presentation.PlayerAnimation;
using ColorTiming.Presentation.UI.Contracts;
using Unity.VisualScripting;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.Events;

public class PlayerActorView : MonoBehaviour, IBattleSessionConsumer, IBattleDamageReceiver, IGameInputConsumer, IGameplayPointerConsumer,
    IGameTimeConsumer, IPlayerDamageSignal, IColorTimingConfigurationConsumer
{
    public event Action Damaged;
    public ActorId DamageActorId => ActorId.Player;
    [NonSerialized] public float moveSpeed = 3.0f;
    [NonSerialized] public float dashSpeed = 5.0f;


    public GameObject characterSprite;
    public Animator animator;
    [SerializeField] private WeaponSpawnerView weaponSpawner;
    [SerializeField] private GameObject deathShow;


    [NonSerialized] public int heroMaxHP = 5;
    public int heroHP => battleSession != null ? battleSession.Snapshot.PlayerHealth : heroMaxHP;
    public GameObject DeathPresentation => deathShow;
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
    internal WeaponIdentity PresentedWeapon => animationDriver != null ? animationDriver.PresentedWeapon : nowweapon;
    internal bool HasPendingWeaponPresentation => animationDriver != null && animationDriver.HasPendingWeaponPresentation;
    AnimatorStateInfo nowAnimStateInfo;
    //动画器跳转事件，1.状态名称 2.true=进入，false=退出
    public UnityEvent<AnimatorStateInfo, bool> OnAnimStateEnter;
    const string animPramNmae_Dash = "Dash";
    const string animPramName_Atk = "Atk";
    const string animPramName_Atk_x = "Atk_x";
    const string animPramName_SwitchWeapon = "switchWeapon";

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
    readonly Dictionary<WeaponIdentity, RuntimeAnimatorController> loadedWeaponControllers =
        new Dictionary<WeaponIdentity, RuntimeAnimatorController>();
    readonly HashSet<WeaponIdentity> loadingWeaponControllers = new HashSet<WeaponIdentity>();
    readonly HashSet<WeaponIdentity> failedWeaponControllers = new HashSet<WeaponIdentity>();
    IPlayerAnimationDriver animationDriver;
    MecanimPlayerAnimationDriver mecanimAnimationDriver;
    bool resolvingWeaponInteraction;
    bool weaponInteractionHandled;
    bool inventorySubscribed;
    bool resourceContextReleased;
    IColorTimingConfiguration configuration;
    ColorTimingPlayerTable playerConfiguration;

    public void BindConfiguration(IColorTimingConfiguration config, ColorTimingSceneId sceneId)
    {
        configuration = config ?? throw new ArgumentNullException(nameof(config));
        var battle = configuration.GetBattle(sceneId);
        playerConfiguration = configuration.GetPlayer(battle.PlayerId);
        moveSpeed = playerConfiguration.MoveSpeed;
        dashSpeed = playerConfiguration.DashSpeed;
        skillMoveSpeed = playerConfiguration.SkillMoveSpeed;
        heroMaxHP = playerConfiguration.MaximumHealth;
    }

    /// <summary>由 BattlePlayerManager 注入不能保存在 Player Prefab 内的场景引用。</summary>
    public void ConfigureSceneReferences(WeaponSpawnerView sceneWeaponSpawner, GameObject deathPresentation)
    {
        weaponSpawner = sceneWeaponSpawner != null
            ? sceneWeaponSpawner
            : throw new ArgumentNullException(nameof(sceneWeaponSpawner));
        deathShow = deathPresentation != null
            ? deathPresentation
            : throw new ArgumentNullException(nameof(deathPresentation));
    }

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
        if (!inventorySubscribed)
        {
            inventory.Changed += OnInventoryWeaponChanged;
            inventorySubscribed = true;
        }
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
        if (playerConfiguration == null)
            throw new InvalidOperationException("Player configuration must be bound before Start.");
        attackInputGate = new PlayerAttackInputGate(
            playerConfiguration.AttackResumeGuard,
            playerConfiguration.HeldAnimatorThreshold);
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
        var initialWeapon = inventory != null ? inventory.Current : nowweapon;
        var initialController = animator != null ? animator.runtimeAnimatorController : null;
        mecanimAnimationDriver = new MecanimPlayerAnimationDriver(animator, initialWeapon, initialController);
        animationDriver = mecanimAnimationDriver;
        if (initialWeapon.IsNormal && initialController != null)
        {
            loadedWeaponControllers[initialWeapon] = initialController;
        }
        foreach (var pair in loadedWeaponControllers)
        {
            mecanimAnimationDriver.RegisterController(pair.Key, pair.Value);
        }
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
        animationDriver.RequestWeapon(nowweapon);
        PreloadWeaponAnimation(nowweapon);
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
        // BattleRuntimeContext deliberately delays session binding until all required
        // controllers are preloaded. Do not consume input or tick gameplay during that gate.
        if (!sessionInitialized || battleSession == null || playerState == null
            || !playerState.IsAlive || !(Time.timeScale > 0))
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
                animationDriver.RequestDash();
            }
        }


        //为了解决暂停恢复通过攻击键
        if (attackInputGate.IsReady)
        {
            if (attackInputGate.ShouldTrigger(gameInput.AttackPressed, playerState.IsAttacking))
            {
                animationDriver.RequestAttack();
            }

            animationDriver.SetAttackHeld(attackInputGate.HeldAnimatorValue(gameInput.AttackHeld));
        }



        if (gameInput.DropPressed && playerState.CanInteractWithWeapons)
        {
            DisWeapon(false);
        }

        TryInstallPendingWeaponController();

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
        var hasMovementInput = inputMove.sqrMagnitude > 0.0001f;
        animationDriver.SetLocomotion(inputMove.magnitude, hasMovementInput);
        if (!playerState.CanMove)
            return;//冲刺中不允移动


        Vector2 _move = Vector2.zero;
        if (!Mathf.Approximately(inputMove.x, 0) || (!Mathf.Approximately(inputMove.y, 0)))
        {
            if (inputMove.magnitude > playerConfiguration.MovementDeadzone)
            {
                _move = inputMove.normalized;

                _move = _move * moveSpeed * Time.deltaTime;

                Vector2 go = transform.position + new Vector3(_move.x, _move.y, 0);
                body.MovePosition(go);
            }


        }
        //print(characterController.velocity.magnitude); //这个值没有停止
        //print(inputMove.magnitude);
        //print(_move.magnitude);
        animationDriver.SetLocomotion(_move.magnitude, hasMovementInput);


    }

    void Dashing()
    {
        if (playerState.CanEvadeDamage)
        {
            Vector2 _dashMove = new Vector2(playerState.FacingX, playerState.DashY);
            //始终为横向赋值
            _dashMove.x = _dashMove.x > 0 ? 1 : -1;
            //检查纵向是否有值
            if (_dashMove.y != 0) _dashMove.y = _dashMove.y > 0
                ? playerConfiguration.DashVerticalScale
                : -playerConfiguration.DashVerticalScale;
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
        // 受击可能先于迟到的 Animation Event 中断攻击；失效事件不得再生成技能伤害。
        if (playerState == null || !playerState.IsAttacking) return;
        heroFrireSystem?.OnFire(nowweapon, characterSprite.transform.localScale.x,parm);
    }

    // 执行PickUP武器对应的主要流程。
    public bool PickUPWeapon(WeaponIdentity weapon)
    {
        // 同一帧可能重叠多个拾取触发器；一次右键只允许其中一个完成换装。
        if (resolvingWeaponInteraction && weaponInteractionHandled)
            return false;

        if (battleSession == null || !battleSession.TryEquipOrSwap(weapon, out var replaced))
        {
            Debug.Log(
                $"[ColorTiming.WeaponInteraction] action=EquipOrSwap result=rejected incoming={weapon} resolving={resolvingWeaponInteraction}",
                this);
            return false;
        }

        weaponInteractionHandled = true;
        Debug.Log(
            $"[ColorTiming.WeaponInteraction] action=EquipOrSwap result=accepted incoming={weapon} replaced={replaced}",
            this);

        if (!replaced.IsNormal)
        {
            weaponSpawner?.CreateWeapon_dis(transform.position, replaced);
        }

        soundManager?.Play(PlayerSoundCue.PickupWeapon);

        return true;
    }
    void RequestWeaponPresentation(WeaponIdentity weapon)
    {
        nowweapon = weapon;
        animationDriver?.RequestWeapon(weapon);
        if (configuration != null)
        {
            if (!loadedWeaponControllers.ContainsKey(weapon))
            {
                PreloadWeaponAnimation(weapon);
                return;
            }
        }

        TryInstallPendingWeaponController();
    }

    /// <summary>Starts loading a candidate controller before the pickup input can be used.</summary>
    public void PreloadWeaponAnimation(WeaponIdentity weapon)
    {
        if (resourceContextReleased || configuration == null || loadedWeaponControllers.ContainsKey(weapon)
            || !loadingWeaponControllers.Add(weapon))
        {
            return;
        }

        string assetName;
        try { assetName = configuration.GetWeapon(weapon).ControllerAsset; }
        catch (Exception exception)
        {
            loadingWeaponControllers.Remove(weapon);
            Debug.LogError($"Player weapon configuration missing for {weapon}: {exception.Message}", this);
            return;
        }
        if (GFBuiltin.Resource == null) { loadingWeaponControllers.Remove(weapon); return; }

        GFBuiltin.Resource.LoadAsset(assetName, typeof(RuntimeAnimatorController), new LoadAssetCallbacks(
            (loadedAssetName, asset, duration, userData) =>
            {
                loadingWeaponControllers.Remove(weapon);
                if (!(asset is RuntimeAnimatorController controller)) return;
                if (resourceContextReleased)
                {
                    GFBuiltin.Resource?.UnloadAsset(controller);
                    return;
                }
                failedWeaponControllers.Remove(weapon);
                loadedWeaponControllers[weapon] = controller;
                mecanimAnimationDriver?.RegisterController(weapon, controller);
                // A resource callback may arrive during attack, dash or hit. It only marks
                // the candidate ready; Update installs it at a stable locomotion boundary.
            },
            (failedAssetName, status, errorMessage, userData) =>
            {
                loadingWeaponControllers.Remove(weapon);
                if (resourceContextReleased) return;
                failedWeaponControllers.Add(weapon);
                Debug.LogError($"Player animation preload failed: {weapon} ({status}) {errorMessage}", this);
            }));
    }

    public void PreloadWeaponAnimations(IReadOnlyList<WeaponIdentity> weapons)
    {
        if (weapons == null) return;
        for (var index = 0; index < weapons.Count; index++) PreloadWeaponAnimation(weapons[index]);
    }

    public bool AreWeaponAnimationsReady(IReadOnlyList<WeaponIdentity> weapons)
    {
        if (weapons == null || configuration == null) return true;
        for (var index = 0; index < weapons.Count; index++)
        {
            if (!loadedWeaponControllers.ContainsKey(weapons[index])) return false;
        }
        return true;
    }

    public int GetReadyWeaponAnimationCount(IReadOnlyList<WeaponIdentity> weapons)
    {
        if (weapons == null) return 0;
        if (configuration == null) return weapons.Count;
        var readyCount = 0;
        for (var i = 0; i < weapons.Count; i++)
        {
            if (loadedWeaponControllers.ContainsKey(weapons[i])) readyCount++;
        }
        return readyCount;
    }

    public bool HasWeaponAnimationPreloadFailure(IReadOnlyList<WeaponIdentity> weapons)
    {
        if (weapons == null || configuration == null) return false;
        for (var index = 0; index < weapons.Count; index++)
        {
            if (failedWeaponControllers.Contains(weapons[index])) return true;
        }
        return false;
    }

    private void TryInstallPendingWeaponController()
    {
        if (animationDriver == null || playerState == null || !animationDriver.HasPendingWeaponPresentation)
        {
            return;
        }

        var weapon = animationDriver.RequestedWeapon;
        if (configuration != null && loadedWeaponControllers.TryGetValue(weapon, out var controller))
        {
            mecanimAnimationDriver.RegisterController(weapon, controller);
        }

        var attackHeld = gameInput != null && attackInputGate != null
            ? attackInputGate.HeldAnimatorValue(gameInput.AttackHeld)
            : 0f;
        if (!animationDriver.TrySynchronizeWeapon(
                playerState.State,
                playerState.IsSkillMoving,
                inputMove.magnitude,
                attackHeld))
        {
            return;
        }

        OnSwitchWeapon?.Invoke(animationDriver.PresentedWeapon);
        Debug.Log(
            $"[ColorTiming.HeroAnimation] action=ControllerInstalled weapon={weapon} version={mecanimAnimationDriver.RequestVersion}",
            this);
    }

    private void ReleasePreloadedWeaponControllers()
    {
        var released = new List<WeaponIdentity>();
        foreach (var pair in loadedWeaponControllers)
        {
            if (!ReferenceEquals(pair.Value, mecanimAnimationDriver?.ActiveController) && !pair.Key.IsNormal)
            {
                GFBuiltin.Resource?.UnloadAsset(pair.Value);
                released.Add(pair.Key);
            }
        }
        foreach (WeaponIdentity weapon in released) loadedWeaponControllers.Remove(weapon);
    }

    private void OnInventoryWeaponChanged(WeaponIdentity weapon)
    {
        RequestWeaponPresentation(weapon);
    }
    //做一个丢弃武器功能  //接通BOSS 攻击
    void DisWeapon(bool dis)
    {
        if (playerState == null || !playerState.CanInteractWithWeapons) return;

        // 手动右键优先让当前范围内的拾取物完成原子换装；没有目标时才丢弃当前武器。
        if (!dis)
        {
            var beforeInteraction = battleSession != null ? battleSession.Inventory.Current : nowweapon;
            resolvingWeaponInteraction = true;
            weaponInteractionHandled = false;
            try
            {
                OnPickUPWeapon?.Invoke();
            }
            finally
            {
                resolvingWeaponInteraction = false;
            }

            Debug.Log(
                $"[ColorTiming.WeaponInteraction] action=ResolveNearby result={(weaponInteractionHandled ? "handled" : "no-target")} before={beforeInteraction} after={(battleSession != null ? battleSession.Inventory.Current : nowweapon)}",
                this);

            if (weaponInteractionHandled)
            {
                return;
            }
        }

        if (battleSession == null || !battleSession.TryDrop(out var dropped))
        {
            return;
        }

        soundManager?.Play(PlayerSoundCue.DropWeapon);
        weaponSpawner?.CreateWeapon_dis(transform.position, dropped);

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
            // 只有仍属于本次有效攻击的正常退出才能消耗一次性武器。
            var completedAttack = playerState != null && playerState.IsAttacking;
            battleSession?.EndAttack();
            if (completedAttack && battleSession != null)
            {
                // Inventory.Changed 是业务武器变化的唯一表现同步入口。
                battleSession.ConsumeAttackWeapon(out _);
            }
        }
        //这个方案延后了
    }

    public void ReceiveDamage(BattleDamage damage)
    {
        if (playerState == null || battleSession == null)
        {
            Debug.LogWarning(
                $"[ColorTiming.Combat][PlayerDamage] action=Resolve result=missing-state attacker={damage.Attacker}",
                this);
            return;
        }

        if (playerState.RejectsDamage)
        {
            Debug.Log(
                $"[ColorTiming.Combat][PlayerDamage] action=Resolve result=rejected-invulnerable attacker={damage.Attacker} state={playerState.State} remainingInvulnerability={playerState.HitInvulnerabilityRemaining:F2}",
                this);
            return;
        }

        if (playerState.CanEvadeDamage)
        {
            Debug.Log(
                $"[ColorTiming.Combat][PlayerDamage] action=Resolve result=evaded attacker={damage.Attacker} state={playerState.State}",
                this);
            gameTime?.Pulse(playerConfiguration.HitTimeScale, playerConfiguration.HitTimeDuration);
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
        Debug.Log(
            $"[ColorTiming.Combat][PlayerDamage] action=Resolve result={resolution} attacker={damage.Attacker} state={playerState.State}",
            this);

        if (resolution == PlayerDamageResolution.Damaged)
        {
            OnSetHP_Event?.Invoke();
            OnDamage_Event?.Invoke();
            Damaged?.Invoke();

            var cPoint = new Vector2(damage.ContactPoint.X, damage.ContactPoint.Y);
            if (cPoint == Vector2.zero) print("计算受击时，获取到一个 0 位置");
            Vector2 cv = (new Vector2(transform.position.x, transform.position.y) - cPoint).normalized;

            //float force = 10;
            Vector2 fc = cv * playerConfiguration.HitKnockback;
            //rigidbody2D.AddForce(cv * force);
            Vector3 af = new Vector3(fc.x,fc.y,0);
            transform.position += af;
            if (!heldWeapon.IsNormal)
            {
                soundManager?.Play(PlayerSoundCue.DropWeapon);
                weaponSpawner?.CreateWeapon_dis(transform.position, heldWeapon);
            }
            animationDriver.RequestHit();
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
            sprite.sortingOrder = playerConfiguration.HitSortingOrder;

            PlayerCameraLifecycleView heroCamera_ = GetComponent<PlayerCameraLifecycleView>();
            heroCamera_.enabled = false;

            animationDriver.RequestDeath();
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
            animationDriver.SetPlaybackSpeed(playerConfiguration.HitAnimatorSpeed);
            hitSlowMotion?.Dispose();
            hitSlowMotion = gameTime?.Acquire(0.1f);

        }
        else
        {
            animationDriver.SetPlaybackSpeed(1f);
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
        resourceContextReleased = true;
        if (inventorySubscribed && inventory != null)
        {
            inventory.Changed -= OnInventoryWeaponChanged;
            inventorySubscribed = false;
        }
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
        ReleasePreloadedWeaponControllers();
    }
}
