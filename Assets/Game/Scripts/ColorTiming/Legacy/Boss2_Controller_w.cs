using Spine;
using Spine.Unity;
using System;
using ColorTiming.Bosses.Boss2;
using ColorTiming.Presentation.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss2_Controller_w : MonoBehaviour, ITransientEntityConsumer
{
    public HeroController hero;
    public Boss2_Controller Boss2_Controller;
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
    Boss2SoundManager soundManager;
    Boss2BurrowFlow burrowFlow;
    ITransientEntityService transientEntities;

    public bool IsStoppedForBattleEnd { get; private set; }

    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }
    private void Start()
    {
        PolygonCollider2D = GetComponent<PolygonCollider2D>();
        soundManager = GetComponentInParent<Boss2SoundManager>();
        burrowFlow = new Boss2BurrowFlow();

        //transform.position = hero.transform.position;
        //AnimPlay(animName_Chutu, false);

        burrowFlow.BeginEntering();
        AnimPlay(animName_Rutu, false);
    }

    private void Update()
    {
        if (IsStoppedForBattleEnd)
        {
            return;
        }

        if (!Boss2_Controller.death)
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

    private void FixedUpdate()
    {
        if (Boss2_Controller == null || Boss2_Controller.death)
        {
            StopForBattleEnd();
            return;
        }

        if (moveing)
        {
            
            //从当前位置移动到hero位置 
            float _dis = Vector2.Distance(transform.position, hero.transform.position);
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
                    int flip = transform.position.x - hero.transform.position.x > 0?1:-1;
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
                transform.position = Vector3.MoveTowards(transform.position, hero.transform.position, moveSpeed * Time.fixedDeltaTime);
            }
        }
    }

    void Think()
    {
        //当与玩家的距离超过 N时，优先使用钻地。 并将其设置到角色脚下
        float _dis = Vector2.Distance(hero.transform.position, transform.position);
        bool b = transform.position.x - hero.transform.position.x > 0 ? transform.localScale.x < 0 : transform.localScale.x > 0; 
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
            soundManager?.PlayBoss2Sound("rt_w");
        }else if(animName == animName_Chutu)
        {
            soundManager?.PlayBoss2Sound("ct_w");
        }else if(animName == animName_Atk1)
        {
            soundManager?.PlayBoss2Sound("atk1_w");
        }
        else if (animName == animName_Atk2)
        {
            soundManager?.PlayBoss2Sound("atk2_w");
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
                        gameObject,
                        new Weapon(ColorType.hong, WeaponType.nor),
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
