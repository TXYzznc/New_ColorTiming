// 文件职责：负责 Boss1Actor 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Application.Battle;
using ColorTiming.Bosses.Boss1;
using ColorTiming.Combat;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using ColorTiming.Input;
using ColorTiming.Presentation.Combat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

public class Boss1ActorView : MonoBehaviour, IBattleDamageReceiver, IBossBattleSessionConsumer, IGameInputConsumer,
    IColorTimingConfigurationConsumer
{
    public ActorId DamageActorId => ActorId.BossHead;
    public BattleKind BattleKind => ColorTiming.Combat.BattleKind.Boss1;

    //boss血量
    //public int hpCount;

    //Boss血量管理器
    //生成随机颜色的血量，
    public List<WeaponColor> Boss1HP { get; private set; }
    public UnityEvent OnDamage_Event;

    public SkeletonAnimation skeletonAnimation1;
    public SkeletonAnimation skeletonAnimation2;
    public SkeletonAnimation skeletonAnimation_tip;
    Boss1AnimationEventRelay boss1Anim;



    const string animName_idle = "idel_60fps";

    const string animName_Hit1 = "hit1_60fps";
    const string animName_Hit2 = "hit2_60fps";

    const string animName_Atk1 = "attack_1_test1_60fps";
    const string animName_Atk2 = "attack_2_test1_60fps";
    const string animName_Atk3 = "attack_3_test2_60fps";
    const string animName_Atk4 = "attack_4_test1_60fps";
    // The original scene used the main Spine animation; the refactor introduced a second authored variant.
    const string animName_Atk5Primary = "attack_5_test1_60fps";
    const string animName_Atk5Secondary = "attack_5_test1_60fps2";
    // 暂时使用主 Spine 动画，等待替换为修正后的四段刺资源。
    const string animName_Atk5 = animName_Atk5Primary;
    const string animName_Atk6 = "attack_6_60fps";

    List<int> HP_zi = new List<int>();
    List<int> HP_hong = new List<int>();
    List<int> HP_lv = new List<int>();

    //string[] hpSpName_zhi = { "紫色11","" };

    Boss1AttackSelector attackSelector;
    Boss1AttackCycle attackCycle;
    BattleSession battleSession;
    bool viewStarted;
    bool sessionInitialized;
    ColorTimingBossTable bossConfiguration;
    IGameInput gameInput;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    bool debugAttack5Active;
    int debugAttack5EventCount;
#endif

    public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
    {
        var battle = configuration?.GetBattle(sceneId) ?? throw new System.ArgumentNullException(nameof(configuration));
        bossConfiguration = configuration.GetBoss(battle.BossId);
        attackSelector = new Boss1AttackSelector(configuration.CreateBoss1AttackRules(battle.BossId));
        attackCycle = new Boss1AttackCycle(bossConfiguration.InitialCooldown);
    }

    /// <summary>Bound by the runtime-created battle composition root before Start.</summary>
    // 绑定战斗会话依赖或事件监听。
    public void BindBattleSession(BattleSession session)
    {
        if (battleSession != null && !ReferenceEquals(battleSession, session))
        {
            throw new System.InvalidOperationException("Boss1 is already bound to another battle session.");
        }
        battleSession = session ?? throw new System.ArgumentNullException(nameof(session));
        TryInitializeSession();
    }

    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new System.ArgumentNullException(nameof(input));
    }
    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        boss1Anim = GetComponent<Boss1AnimationEventRelay>();
        //skeletonAnimation1 = GetComponentInChildren<SkeletonAnimation>();
        AnimPlay(animName_idle, true);
        //skeletonAnimation1.Start();
        viewStarted = true;
        TryInitializeSession();

        //Guandeng("紫色11");
        //Guandeng("红色3");
        //Guandeng("绿色1");


    }

    // 尝试Initialize会话，并通过返回值报告是否成功。
    void TryInitializeSession()
    {
        if (sessionInitialized || !viewStarted || battleSession == null) return;
        sessionInitialized = true;
        CreateBossHP();
    }

    // Update is called once per frame
    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        HandleAttack5DebugInput();
        if (debugAttack5Active)
        {
            return;
        }
#endif
        if (attackCycle == null || attackSelector == null || battleSession == null
            || battleSession.Snapshot.Lifecycle != BattleLifecycle.Running)
        {
            return;
        }

        if (attackCycle.Tick(Time.deltaTime) && attackCycle.BeginAttack())
        {
            ThinkAtk();
        }
    }

    bool ca1 = false;
    bool ca2 = false;
    // 执行EnterCase对应的主要流程。
    public void EnterCase(int cI, bool enter)
    {
        if (cI == 1)
        {
            ca1 = enter;
        }
        else
        {
            ca2 = enter;
        }
        //print(cI + "角色进出Boss检测" + enter);
        if (enter)
        {
            //非常容易多次调用
            //atkCD -= Random.Range(0.2f, 0.7f);
        }
    }

    //Boss决策与攻击逻辑
    void ThinkAtk()
    {
        var zone = Boss1DistanceZones.Resolve(ca1, ca2);
        var attack = attackSelector.Select(zone, Random.value);
        AnimPlay(GetAnimationName(attack), false);
    }

    static string GetAnimationName(Boss1Attack attack)
    {
        switch (attack)
        {
            case Boss1Attack.Attack1: return animName_Atk1;
            case Boss1Attack.Attack2: return animName_Atk2;
            case Boss1Attack.Attack3: return animName_Atk3;
            case Boss1Attack.Attack4: return animName_Atk4;
            case Boss1Attack.Attack5: return animName_Atk5;
            case Boss1Attack.Attack6: return animName_Atk6;
            default: throw new System.ArgumentOutOfRangeException(nameof(attack));
        }
    }


    void AnimPlay(string animName, bool loop)
    {
        SkeletonAnimation _skAni;
        if (animName == animName_Atk5Secondary)
        {
            _skAni = skeletonAnimation2;
            skeletonAnimation1.gameObject.SetActive(false);
            skeletonAnimation_tip.gameObject.SetActive(false);
            skeletonAnimation2.gameObject.SetActive(true);
        }
        else
        {
            _skAni = skeletonAnimation1;
            skeletonAnimation1.gameObject.SetActive(true);
            skeletonAnimation_tip.gameObject.SetActive(true);
            skeletonAnimation_tip.AnimationState.SetAnimation(0, animName, loop);
            skeletonAnimation2.gameObject.SetActive(false);
        }




        TrackEntry entry = _skAni.AnimationState.SetAnimation(0, animName, loop);
        //entry.End += Entry_End;
        if (animName != animName_idle)
            entry.Complete += Entry_Complete;
        entry.Event += AnimEvent;
        entry.End += Entry_End;

        if(animName == animName_Atk1)
        {
            boss1Anim?.PlaySound("atk1");
        }else if(animName == animName_Atk2)
        {
            boss1Anim?.PlaySound("atk2");
        }
        else if (animName == animName_Atk3)
        {
            boss1Anim?.PlaySound("atk3_1");
        }
        else if(animName == animName_Atk4)
        {
            boss1Anim?.PlaySound("atk4");
        }else if(animName == animName_Atk6)
        {
            boss1Anim?.PlaySound("atk6");
        }
    }

    private void AnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        //if (e.ToString() == "attack")
        //{
        //    print("检测到Boss攻击事件");
        //}
        boss1Anim?.GoAtk(trackEntry, e);
        if (e.ToString() == "attack" && e.String == "atk5")
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugAttack5Active)
            {
                debugAttack5EventCount++;
                Debug.Log(
                    $"[ColorTiming.Boss1Attack5Debug] action=AnimationEvent variant={trackEntry.Animation.Name} event=atk5 count={debugAttack5EventCount}",
                    this);
            }
#endif
            battleSession.SetBossDamageable(false);
            OffAllHPColor();
            //进入无敌
        }
    }

    private void Entry_Complete(TrackEntry trackEntry)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (debugAttack5Active)
        {
            Debug.Log(
                $"[ColorTiming.Boss1Attack5Debug] action=Completed variant={trackEntry.Animation.Name} atk5EventCount={debugAttack5EventCount}",
                this);
            debugAttack5Active = false;
        }
#endif
        if (!skeletonAnimation1.gameObject.activeSelf)
        {
            skeletonAnimation1.gameObject.SetActive(true);
            skeletonAnimation_tip.gameObject.SetActive(true);
            skeletonAnimation2.gameObject.SetActive(false);
            //skeletonAnimation1.Start();
        }

        attackCycle.CompleteAttack(Random.Range(bossConfiguration.NextCooldownMin, bossConfiguration.NextCooldownMax));
        AnimPlay(animName_idle, true);
        OnHPColor();
        battleSession.SetBossDamageable(true);

    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void HandleAttack5DebugInput()
    {
        if (gameInput == null || battleSession == null
            || battleSession.Snapshot.Lifecycle != BattleLifecycle.Running)
        {
            return;
        }

        if (gameInput.DebugBoss1Attack5PrimaryPressed)
        {
            TryPlayAttack5Debug(animName_Atk5Primary, "primary");
        }
        else if (gameInput.DebugBoss1Attack5SecondaryPressed)
        {
            TryPlayAttack5Debug(animName_Atk5Secondary, "secondary");
        }
    }

    private void TryPlayAttack5Debug(string animationName, string variant)
    {
        if (debugAttack5Active || attackCycle == null || attackCycle.IsAttacking)
        {
            Debug.Log(
                $"[ColorTiming.Boss1Attack5Debug] action=Play result=ignored variant={variant} active={debugAttack5Active} cycleAttacking={attackCycle != null && attackCycle.IsAttacking}",
                this);
            return;
        }

        debugAttack5Active = true;
        debugAttack5EventCount = 0;
        Debug.Log($"[ColorTiming.Boss1Attack5Debug] action=Play result=started variant={variant} animation={animationName}", this);
        AnimPlay(animationName, false);
    }

    [ContextMenu("ColorTiming/Debug/Attack5 Primary (Key 1)")]
    private void PlayAttack5PrimaryFromContextMenu()
    {
        TryPlayAttack5Debug(animName_Atk5Primary, "primary");
    }

    [ContextMenu("ColorTiming/Debug/Attack5 Secondary (Key 2)")]
    private void PlayAttack5SecondaryFromContextMenu()
    {
        TryPlayAttack5Debug(animName_Atk5Secondary, "secondary");
    }
#endif

    private void Entry_End(TrackEntry trackEntry)
    {
        trackEntry.Event -= AnimEvent;
        trackEntry.Complete -= Entry_Complete;
        trackEntry.End -= Entry_End;
    }

    // 创建BossHP并完成必要的初始配置。
    void CreateBossHP()
    {
        if (battleSession == null || battleSession.Kind != BattleKind.Boss1)
        {
            throw new System.InvalidOperationException("Boss1 requires a Boss1 BattleSession before Start.");
        }
        SyncCompatibilityHealth();


        HP_hong.Add(3);
        HP_hong.Add(4);
        HP_hong.Add(7);
        HP_hong.Add(8);
        HP_hong = RandomSort(HP_hong);


        HP_zi.Add(11);
        HP_zi.Add(2);
        HP_zi.Add(5);
        HP_zi.Add(9);
        HP_zi = RandomSort(HP_zi);


        HP_lv.Add(1);
        HP_lv.Add(10);
        HP_lv.Add(6);
        HP_lv = RandomSort(HP_lv);

        OffAllHPColor();

        //int[] HP_zhi = {11,2,5,9 };
        //int[] HP_hong = {3,4,7,8 };
        //int[] HP_lv = {1,10,6 };


        OnHPColor();


        print("已初始化BOSS血量:" + Boss1HP.Count);


        OnDamage_Event?.Invoke();
        if (boss1Anim != null)
        {
            OnDamage_Event.AddListener(boss1Anim.OnHit);
        }

    }

    // 执行Receive伤害对应的主要流程。
    public void ReceiveDamage(BattleDamage damage)
    {
        if (battleSession == null)
        {
            return;
        }

        var before = battleSession.Snapshot;
        var expectedColor = before.Weaknesses.Count > 0 ? before.Weaknesses[0].ToString() : "None";
        var resolution = battleSession.ApplyBossDamage(damage);
        Debug.Log(
            $"[ColorTiming.Combat][BossDamage] action=Resolve boss=Boss1 result={resolution} attacker={damage.Attacker} weapon={damage.Weapon.Color} expected={expectedColor} remaining={battleSession.Snapshot.Weaknesses.Count}",
            this);
        if (resolution == BossDamageResolution.RejectedWrongColor)
        {
            print("颜色不同，不造成伤害");
            return;
        }
        if (resolution == BossDamageResolution.RejectedInvulnerable
            || resolution == BossDamageResolution.RejectedCompleted)
        {
            return;
        }

        SyncCompatibilityHealth();
        int rHit = Random.Range(0, 2);
        string playn = rHit > 0 ? animName_Hit1 : animName_Hit2;

        AnimPlay(playn, false);
        OffAllHPColor();
        DamageHPColor(damage.Weapon.Color, 0);
        OnHPColor();
        OnDamage_Event?.Invoke();

        if (resolution == BossDamageResolution.Victory)
            print("BOSS 已经死亡，不再处理伤害");
    }

    void SyncCompatibilityHealth()
    {
        Boss1HP = battleSession.Snapshot.Weaknesses
            .ToList();
    }


    private List<T> RandomSort<T>(List<T> list)
    {
        var random = new System.Random();
        var newList = new List<T>();
        foreach (var item in list)
        {
            newList.Insert(random.Next(newList.Count), item);
        }
        return newList;
    }


    void DamageHPColor(WeaponColor color, float c)
    {
        string slotName = "";
        switch (color)
        {
            case WeaponColor.Red:
                slotName = "红色" + HP_hong[0];
                HP_hong.RemoveAt(0);

                //HP_hong
                break;
            case WeaponColor.Green:
                slotName = "绿色" + HP_lv[0];
                HP_lv.RemoveAt(0);
                break;
            case WeaponColor.Purple:
                slotName = "紫色" + HP_zi[0];
                HP_zi.RemoveAt(0);
                break;
            default:
                break;
        }

        SetHPColor(slotName, c);


    }

    // 响应HP颜色回调，并更新本对象状态。
    void OnHPColor()
    {
        if(Boss1HP.Count > 0)
        {
            SetHPColor(Boss1HP[0], 1);
        }
        else
        {
            //已胜利
        }
    }

    void OffAllHPColor()
    {
        SetHPColor(WeaponColor.Red,0.2f);
        SetHPColor(WeaponColor.Purple, 0.2f);
        SetHPColor(WeaponColor.Green, 0.2f);
    }

    // 设置HP颜色，并使后续流程使用最新状态。
    void SetHPColor(WeaponColor color,float c)
    {
        switch (color)
        {
            case WeaponColor.Red:
                foreach (var type in HP_hong)
                {
                    string s = "红色" + type;
                    SetHPColor(s, c);
                }
                break;
            case WeaponColor.Green:
                foreach (var type in HP_lv)
                {
                    string s = "绿色" + type;
                    SetHPColor(s, c);
                }
                break;
            case WeaponColor.Purple:
                foreach (var type in HP_zi)
                {
                    string s = "紫色" + type;
                    SetHPColor(s, c);
                }
                break;
            default:
                break;
        }
    }

    // 设置HP颜色，并使后续流程使用最新状态。
    void SetHPColor(string _slot,float c)
    {
        var slot1 = skeletonAnimation1.skeleton.FindSlot(_slot);
        //if (slot1 != null) print(slot1.ToString());
        if (slot1 == null)
        {
            print("没找到骨骼插槽");
            return;
        }
        float tsc = c;
        slot1.SetColor(new Color(tsc, tsc, tsc,1));



    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        if (boss1Anim != null)
        {
            OnDamage_Event.RemoveListener(boss1Anim.OnHit);
        }
    }

    private sealed class UnityRandomSource : IRandomSource
    {
        // 执行Range对应的主要流程。
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return Random.Range(minimumInclusive, maximumExclusive);
        }
    }



}
