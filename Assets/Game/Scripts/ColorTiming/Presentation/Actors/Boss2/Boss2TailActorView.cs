// 文件职责：负责 Boss2尾部Actor 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Boss2。

using Spine;
using Spine.Unity;
using System;
using ColorTiming.Application.Battle;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Combat;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Boss2TailActorView : MonoBehaviour, ITransientEntityConsumer, IBattleSessionConsumer, IPlayerTargetConsumer
{
    Transform playerTarget;
    BattleSession battleSession;
    public SkeletonAnimation skeletonAnimation;
    public SpriteRenderer sprite;

    public GameObject sk1;
    public GameObject sk2;

    public Transform mao1;
    public Transform mao2;

    public GameObject skY;

    const string animName_idle = "idel";

    const string animName_Hit = "ShouJi";

    const string animName_Rutu = "RuTu";
    const string animName_Chutu = "ChuTu";

    const string animName_Atk1 = "attack_1";
    const string animName_Atk2 = "attack_2";


    public float moveSpeed = 25;

    float atkCD = 1;
    bool attacking;
    bool moveing;
    float moveOkTimeing;

    bool first;

    PolygonCollider2D PolygonCollider2D;
    Boss2SoundView soundManager;
    Boss2BurrowFlow burrowFlow;
    ITransientEntityService transientEntities;

    public bool IsStoppedForBattleEnd { get; private set; }

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 绑定战斗会话依赖或事件监听。
    public void BindBattleSession(BattleSession session)
    {
        if (battleSession != null && !ReferenceEquals(battleSession, session))
            throw new InvalidOperationException("Boss2 tail is already bound to another battle session.");
        battleSession = session ?? throw new ArgumentNullException(nameof(session));
    }

    // 绑定玩家目标依赖或事件监听。
    public void BindPlayerTarget(Transform target)
    {
        playerTarget = target != null ? target : throw new ArgumentNullException(nameof(target));
    }
    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        PolygonCollider2D = GetComponent<PolygonCollider2D>();
        soundManager = GetComponentInParent<Boss2SoundView>();
        burrowFlow = new Boss2BurrowFlow();

        //transform.position = hero.transform.position;
        //AnimPlay(animName_Chutu, false);

        burrowFlow.BeginEntering();
        AnimPlay(animName_Rutu, false);
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {
        if (IsStoppedForBattleEnd || battleSession == null || playerTarget == null)
        {
            return;
        }

        if (battleSession != null && battleSession.Snapshot.Lifecycle == BattleLifecycle.Running)
        {
            if (atkCD > 0)
            {
                atkCD -= Time.deltaTime;
            }else if (!attacking)
            {
                Think();
            }
        }
    }

    // 按物理帧推进与刚体或碰撞相关的状态。
    private void FixedUpdate()
    {
        if (battleSession == null || playerTarget == null)
        {
            return;
        }
        if (battleSession.Snapshot.Lifecycle != BattleLifecycle.Running)
        {
            StopForBattleEnd();
            return;
        }

        if (moveing)
        {

            //从当前位置移动到hero位置
            float _dis = Vector2.Distance(transform.position, playerTarget.position);
            float _fd = first ? 5 : 10;
            float _fdt = first ? 0.5f : 2f;
            if (_dis < 5)
            {

                PolygonCollider2D.enabled = true;
                //进入准备钻出程序
                if (moveOkTimeing < _fdt)
                {

                    sprite.gameObject.SetActive(true);
                    moveOkTimeing += Time.fixedDeltaTime;
                    float _f = Mathf.Lerp(0,1,moveOkTimeing / 0.5f);


                    sprite.color = new Color(1, 1, 1, _f);
                }
                else
                {
                    sprite.gameObject.SetActive(false);
                    //设置转向Hero   >0时  正向，
                    int flip = transform.position.x - playerTarget.position.x > 0?1:-1;
                    transform.localScale = new Vector3(flip,1,1);


                    moveing = false;
                    burrowFlow.BeginEmerging();
                    AnimPlay(animName_Chutu, false);
                    skY.SetActive(true);
                }
            }
            else
            {
                first = true;
                PolygonCollider2D.enabled = false;
                transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.fixedDeltaTime);
            }
        }
    }

    void Think()
    {
        //当与玩家的距离超过 N时，优先使用钻地。 并将其设置到角色脚下
        float _dis = Vector2.Distance(playerTarget.position, transform.position);
        bool b = transform.position.x - playerTarget.position.x > 0 ? transform.localScale.x < 0 : transform.localScale.x > 0;
        var action = Boss2ActionSelector.SelectTail(_dis, b, Random.value);
        if (action == Boss2Action.Burrow)
        {
            moveOkTimeing = 0;

            //使用钻入地下
            burrowFlow.BeginEntering();
            AnimPlay(animName_Rutu,false);
        }
        else
        {
            AnimPlay(
                action == Boss2Action.Melee ? animName_Atk1 : animName_Atk2,
                false);

        }

        attacking = true;
    }

    void AnimPlay(string animName, bool loop)
    {


        TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
        //entry.End += Entry_End;
        if (animName != animName_idle)
            entry.Complete += Entry_Complete;
        entry.Event += AnimEvent;
        entry.End += Entry_End;
        if(animName == animName_Rutu)
        {
            soundManager?.Play(Boss2SoundCue.TailEnterBurrow);
        }else if(animName == animName_Chutu)
        {
            soundManager?.Play(Boss2SoundCue.TailExitBurrow);
        }else if(animName == animName_Atk1)
        {
            soundManager?.Play(Boss2SoundCue.TailAttack1);
        }
        else if (animName == animName_Atk2)
        {
            soundManager?.Play(Boss2SoundCue.TailAttack2);
        }


    }

    private void Entry_Complete(TrackEntry trackEntry)
    {

        //print("动画已结束" + atkCD);

        trackEntry.Complete -= Entry_Complete;

        if(trackEntry.Animation.Name == animName_Rutu)
        {
            skY.SetActive(false);
            burrowFlow.EnterHiddenMovement();
            moveing = true;
        }
        //else if (trackEntry.Animation.Name == animName_Chutu)
        //{
        //    AnimPlay(animName_idle, false);
        //    attacking = false;
        //    atkCD = Random.Range(1.0f, 2.5f);
        //}
        else
        {
            if (trackEntry.Animation.Name == animName_Chutu)
            {
                burrowFlow.CompleteEmerging();
            }
            AnimPlay(animName_idle, true);
            attacking = false;
            atkCD = Random.Range(1.0f, 2.5f);
        }


    }

    // 停止For战斗结束并清理临时播放状态。
    public void StopForBattleEnd()
    {
        if (IsStoppedForBattleEnd)
        {
            return;
        }

        IsStoppedForBattleEnd = true;
        attacking = false;
        moveing = false;
        moveOkTimeing = 0f;
        burrowFlow?.Interrupt();
        if (PolygonCollider2D != null)
        {
            PolygonCollider2D.enabled = false;
        }
        if (sprite != null)
        {
            sprite.gameObject.SetActive(false);
        }
        if (skY != null)
        {
            skY.SetActive(false);
        }
    }

    private void Entry_End(TrackEntry trackEntry)
    {
        trackEntry.Event -= AnimEvent;
        trackEntry.Complete -= Entry_Complete;
        trackEntry.End -= Entry_End;
    }

    private void AnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        //已使用永久伤害

        //return;

        if (e.ToString() != "attack") return;
         GameObject wsk = null;
        Transform wmao = null;
        string parm = "";

        if (e.String == "atk1")
        {
            //soundManager?.PlayBoss2Sound("atk1_w");
            wsk = sk1;
            wmao = transform;
        }
        else if(e.String != "")
        {
            //soundManager?.PlayBoss2Sound("atk2_w");
            wsk = sk2;
            wmao = transform;
        }

        if (wsk && wmao)
        {
            if (transientEntities == null)
            {
                throw new InvalidOperationException("Boss2 tail entities were not bound by the composition root.");
            }
            int _flip = transform.localScale.x > 0 ? 1 : -1;
            transientEntities.Spawn(
                wsk.name,
                wmao.position,
                wmao.rotation,
                wmao,
                instance =>
                {
                    instance.transform.localPosition = wsk.transform.localPosition;
                    instance.transform.localRotation = wsk.transform.localRotation;
                    instance.transform.localScale = wsk.transform.localScale;
                    instance.GetComponent<Skill_base>()?.SetSkillData(
                        ActorId.BossTail,
                        new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Normal),
                        _flip,
                        parm);
                    instance.GetComponent<Skill_Bo2w_Atk>()?.BindTail(skeletonAnimation);
                });

        }
        else
        {
            print("未正确创建技能:" + e.String);
        }

    }


}
