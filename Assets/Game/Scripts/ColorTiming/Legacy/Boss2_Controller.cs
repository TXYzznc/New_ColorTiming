using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class Boss2_Controller : MonoBehaviour, I_Damage, ITransientEntityConsumer, IBattleResultConsumer
{
    IBattleResultSink battleResultSink;

    //boss血量
    //public int hpCount;

    //Boss血量管理器
    //生成随机颜色的血量，
    public List<ColorType> Boss1HP { get; private set; }
    public UnityEvent OnDamage_Event;

    public SkeletonAnimation skeletonAnimation1;
    public SkeletonAnimation skeletonAnimation_tu;
    Boss2Anim_s boss2Anim_s;

    public HeroController hero;

    public GameObject dundiObj;

    public Transform randomMoveT;
    Vector3 randomMove;

    public Boss2_Controller_w boss2_Controller_W;

    const string animName_idle = "idel";

    const string animName_Hit = "ShouJi";

    const string animName_Rutu = "RuTu";
    const string animName_Chutu = "ChuTu";

    const string animName_Atk1 = "attack_1";
    const string animName_Atk2 = "attack_2";



    List<int> HP_zi = new List<int>();
    List<int> HP_hong = new List<int>();
    List<int> HP_lv = new List<int>();
    List<int> HP_chen = new List<int>();

    //string[] hpSpName_zhi = { "紫色11","" };

    float atkCD = 3;

    float moveSpeed = 5;

    PolygonCollider2D polygonCollider2D;
    public bool death { get; private set; }
    WeaknessQueue weaknessQueue;
    BossBattleHealth battleHealth;
    Boss2PhaseCoordinator phaseCoordinator;
    Boss2BurrowFlow burrowFlow;
    ITransientEntityService transientEntities;
    WeaknessSlotLedger slotLedger;

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    public void BindBattleResultSink(IBattleResultSink sink)
    {
        battleResultSink = sink ?? throw new ArgumentNullException(nameof(sink));
    }
    void Start()
    {
        boss2Anim_s = GetComponent<Boss2Anim_s>();
        burrowFlow = new Boss2BurrowFlow();
        
        CreateBossHP();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    bool attacking;
    // Update is called once per frame
    void Update()
    {
        if (death)
        {
            return;
        }

        if (atkCD > 0)
        {
            atkCD -= Time.deltaTime;
           //print(atkCD);
        }
        else
        {
            if (!attacking)
            {
                //print("ttttttttttttt" + atkCD);
                //atkCD = Random.Range(2,6);
                
                ThinkAtk();
            }

        }
    }

    bool waitMoveEnd;
    bool moveing;
    float moveOkTimeing;
    float dundiTime;

    float lastCaseDis;
    private void FixedUpdate()
    {
        if (death)
        {
            moveing = false;
            return;
        }

        if (moveing)
        {
           
            //float _dis = Vector2.Distance(transform.position, hero.transform.position);
            float _dis = Vector2.Distance(transform.position, randomMove);
            if (_dis < 1)
            {
                //进入准备钻出程序
                waitMoveEnd = true;        
            }


            if (waitMoveEnd)
            {

                if (moveOkTimeing < 1)
                {
                    moveOkTimeing += Time.fixedDeltaTime;
                    dundiTime -= Time.fixedDeltaTime;
                    if (dundiTime < 0)
                    {
                        dundiTime = 0.3f;
                        CreateDundi();
                    }
                    //float _f = Mathf.Lerp(0, 1, moveOkTimeing / 1);
                }
                else
                {
                    //设置转向Hero   >0时  反向，
                    int flip = transform.position.x - hero.transform.position.x > 0 ? -1 : 1;
                    transform.localScale = new Vector3(flip, 1, 1);

                    burrowFlow.BeginEmerging();
                    AnimPlay(animName_Chutu, false);
                    skeletonAnimation_tu.gameObject.SetActive(false);

                    moveOkTimeing = 0;
                    moveing = false;
                    waitMoveEnd = false;

                    CreateDundi();
                    lastCaseDis = 0;
                }
            }
            else
            {
                //transform.position = Vector3.MoveTowards(transform.position, hero.transform.position, moveSpeed * Time.fixedDeltaTime);
                transform.position = Vector3.MoveTowards(transform.position, randomMove, moveSpeed * Time.fixedDeltaTime);

                if (lastCaseDis > 0)
                {
                    if (lastCaseDis - _dis > 1.5f)
                    {
                        CreateDundi();
                        lastCaseDis = _dis;
                    }
                }
                else
                {
                    lastCaseDis = _dis;
                }

            }

            //if (dundiTime > 0f) 
            //{
            //    dundiTime -= Time.fixedDeltaTime;
            //}
            //else
            //{
            //    dundiTime = 0.3f;
            //    CreateDundi();
            //}
        }


    }

    bool ca1 = false;
    bool ca2 = false;
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
        attacking = true;
        float _dis = Vector2.Distance(hero.transform.position, transform.position);
        bool b = transform.position.x - hero.transform.position.x < 0 ? transform.localScale.x < 0 : transform.localScale.x > 0;
        var action = Boss2ActionSelector.SelectHead(_dis, b, Random.value);
        switch (action)
        {
            case Boss2Action.Burrow:
                burrowFlow.BeginEntering();
                AnimPlay(animName_Rutu, false);
                break;
            case Boss2Action.Melee:
                AnimPlay(animName_Atk1, false);
                break;
            default:
                AnimPlay(animName_Atk2, false);
                break;
        }

    }

    string nowAnim;
    void AnimPlay(string animName, bool loop)
    {
        SkeletonAnimation _skAni = null;
        //已弃用sp2
        _skAni = skeletonAnimation1;

        TrackEntry entry = _skAni.AnimationState.SetAnimation(0, animName, loop);
        //entry.End += Entry_End;
        if (animName != animName_idle)
            entry.Complete += Entry_Complete;
        entry.Event += AnimEvent;
        entry.End += Entry_End;

        nowAnim = animName;

        if(animName == animName_Rutu)
        {
            boss2Anim_s.Rutu();
        }

        if(animName == animName_Atk1)
        {
            boss2Anim_s.PlaySound("atk1");
        }else if(animName == animName_Atk2)
        {
            boss2Anim_s.PlaySound("atk2");
        }

    }

    private void AnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        //if (e.ToString() == "attack")
        //{
        //    print("检测到Boss攻击事件");
        //}
        boss2Anim_s.GoAtk(trackEntry,e);
        if (e.ToString() == "attack")
        {
            if(e.String == "atk0")
            {
                polygonCollider2D.enabled = true;
            }
        }
    }

    private void Entry_Complete(TrackEntry trackEntry)
    {
        if (death)
        {
            trackEntry.Complete -= Entry_Complete;
            return;
        }
        if (!skeletonAnimation1.gameObject.activeSelf)
        {
            //skeletonAnimation1.gameObject.SetActive(true);
            //skeletonAnimation_tip.gameObject.SetActive(true);
            //skeletonAnimation1.Start();
        }

        
       
        //print("动画已结束" + atkCD);

        trackEntry.Complete -= Entry_Complete;


        if (trackEntry.Animation.Name == animName_Rutu)
        {
            SetrRandomMove();
            burrowFlow.EnterHiddenMovement();
            moveing = true;
            //skeletonAnimation_tu.gameObject.SetActive(true);
            CreateDundi();
            polygonCollider2D.enabled = false;
        }
        else
        {
            if (trackEntry.Animation.Name == animName_Chutu)
            {
                burrowFlow.CompleteEmerging();
            }
            attacking = false;
            AnimPlay(animName_idle, true);
            OnHPColor();
            atkCD = Random.Range(1.5f, 3.0f);
        }

    }

    void CreateBossHP()
    {
        weaknessQueue = WeaknessQueue.CreateBoss2(new UnityRandomSource());
        battleHealth = new BossBattleHealth(weaknessQueue);
        phaseCoordinator = new Boss2PhaseCoordinator(weaknessQueue.Count);
        SyncCompatibilityHealth();


        for (int i = 1; i < 6; i++) 
        {
            HP_hong.Add(i);
        }
        HP_hong = RandomSort(HP_hong);


        for (int i = 1; i < 7; i++)
        {
            HP_zi.Add(i);
        }
        HP_zi = RandomSort(HP_zi);


        for (int i = 1; i < 7; i++)
        {
            HP_lv.Add(i);
        }
        HP_lv = RandomSort(HP_lv);


        for (int i = 1; i < 6; i++)
        {
            HP_chen.Add(i);
        }
        HP_chen = RandomSort(HP_chen);

        slotLedger = new WeaknessSlotLedger(new Dictionary<WeaponColor, IEnumerable<int>>
        {
            [WeaponColor.Red] = HP_hong,
            [WeaponColor.Green] = HP_lv,
            [WeaponColor.Purple] = HP_zi,
            [WeaponColor.Orange] = HP_chen,
        });

        OffAllHPColor();

        //int[] HP_zhi = {11,2,5,9 };
        //int[] HP_hong = {3,4,7,8 };
        //int[] HP_lv = {1,10,6 };


        {
            //放弃纯随机
            //for (int i = 0; i < count; i++) {
            //    int t = Random.Range(1, 3);
            //    ColorType ctp = ColorType.hong;
            //    switch (t)
            //    {
            //        case 1:
            //            ctp = ColorType.hong;
            //            break;
            //            case 2:
            //                ctp = ColorType.lv;
            //            break;
            //        case 3:
            //            ctp = ColorType.zi;
            //            break;
            //        default:
            //            break;
            //    }

            //    Boss1HP.Add(ctp);
            //}
        }

        OnHPColor();


        print("已初始化BOSS血量:" + Boss1HP.Count);

        
        OnDamage_Event?.Invoke();
        if (boss2Anim_s != null)
        {
            OnDamage_Event.AddListener(boss2Anim_s.OnHit);
        }

    }

    public void OnDamage(GameObject _attacker, Weapon atkWapon, Vector2 cPoint, string parm)
    {
        if (battleHealth == null || atkWapon == null)
        {
            return;
        }

        var request = new DamageRequest(
            _attacker,
            atkWapon.Identity,
            new CombatPoint(cPoint.x, cPoint.y),
            parm);
        var resolution = battleHealth.Apply(request);
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

        if (phaseCoordinator.ObserveRemaining(Boss1HP.Count))
        {
            boss2_Controller_W?.gameObject.SetActive(true);
        }
   
        if(nowAnim != animName_Rutu) AnimPlay(animName_Hit, false);


        OffAllHPColor();
        DamageHPColor(atkWapon.colorType, 0);


        if (Boss1HP.Count > 0)
        {
            OnHPColor();
        }

        OnDamage_Event?.Invoke();

        if (resolution == BossDamageResolution.Victory)
        {
            print("BOSS 已经死亡，不再处理伤害");
            death = true;
            burrowFlow.Interrupt();
            moveing = false;
            attacking = false;
            if (polygonCollider2D != null)
            {
                polygonCollider2D.enabled = false;
            }
            boss2_Controller_W?.StopForBattleEnd();
            transientEntities?.ReleaseAll();
            battleResultSink?.Show(BattlePresentationResult.FinalVictory);
        }
    }

    private void Entry_End(TrackEntry trackEntry)
    {
        trackEntry.Event -= AnimEvent;
        trackEntry.Complete -= Entry_Complete;
        trackEntry.End -= Entry_End;
    }

    void SyncCompatibilityHealth()
    {
        Boss1HP = weaknessQueue.Upcoming(weaknessQueue.Count)
            .Select(color => (ColorType)color)
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


    void DamageHPColor(ColorType color, float c)
    {
        var weaponColor = (WeaponColor)color;
        string slotName = GetSlotPrefix(weaponColor) + slotLedger.Consume(weaponColor);
        SetHPColor(slotName, c,0);


    }

    void OnHPColor()
    {
        if (Boss1HP.Count > 0)
        {
            SetHPColor(Boss1HP[0], 1, 1);
        }

    }

    void OffAllHPColor()
    {
        SetHPColor(ColorType.hong,0.1f,0.1f);
        SetHPColor(ColorType.zi, 0.1f, 0.1f);
        SetHPColor(ColorType.lv, 0.1f, 0.1f);
        SetHPColor(ColorType.chen, 0.1f, 0.1f);
    }

    void SetHPColor(ColorType color,float c,float a)
    {
        var weaponColor = (WeaponColor)color;
        foreach (var type in slotLedger.Remaining(weaponColor))
        {
            SetHPColor(GetSlotPrefix(weaponColor) + type, c, a);
        }
    }

    static string GetSlotPrefix(WeaponColor color)
    {
        switch (color)
        {
            case WeaponColor.Red: return "红色";
            case WeaponColor.Green: return "绿色";
            case WeaponColor.Purple: return "紫色";
            case WeaponColor.Orange: return "橙色";
            default: throw new ArgumentOutOfRangeException(nameof(color));
        }
    }

    void SetHPColor(string _slot,float c,float a)
    {
        //这个Boss不确定颜色骨骼名称
        //return;

        //print( c + "设置血量颜色 :" + _slot);
        var slot1 = skeletonAnimation1.skeleton.FindSlot(_slot);
        //if (slot1 != null) print(slot1.ToString());
        if (slot1 == null)
        {
            print("没找到骨骼插槽:" + _slot);
            return;
        }
        float tsc = c;
        slot1.SetColor(new Color(tsc, tsc, tsc,a));

        

    }

   
    public void CreateDundi()
    {
        if (!moveing || dundiObj == null) return;
        if (transientEntities == null)
        {
            throw new InvalidOperationException("Boss2 burrow trail entities were not bound by the composition root.");
        }
        transientEntities.Spawn(
            dundiObj.name,
            transform.position,
            Quaternion.identity,
            null,
            null);
    }

    void SetrRandomMove()
    {
        List<Vector3> rmV3 = new List<Vector3>();
        foreach (Transform item in randomMoveT)
        {
            float _d =  Vector2.Distance(item.position,transform.position);
            if (_d > 5) 
            {
                rmV3.Add(item.position);
            }
        }

        if (rmV3.Count == 0)
        {
            Debug.LogWarning("Boss2 has no relocation anchor farther than five units; emerging in place.", this);
            randomMove = transform.position;
            return;
        }

        randomMove = rmV3[Random.Range(0, rmV3.Count)];
    }

    private void OnDestroy()
    {
        if (boss2Anim_s != null)
        {
            OnDamage_Event.RemoveListener(boss2Anim_s.OnHit);
        }
        burrowFlow?.Interrupt();
    }

    private sealed class UnityRandomSource : IRandomSource
    {
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return Random.Range(minimumInclusive, maximumExclusive);
        }
    }

}


