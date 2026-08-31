// 文件职责：定义 Boss2动画事件Relay，承担 Boss2 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Boss2。

using Spine;
using System;
using ColorTiming.Combat;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Entities;
using UnityEngine;


public class Boss2AnimationEventRelay : MonoBehaviour, ITransientEntityConsumer, IPlayerTargetConsumer
{
    public GameObject sk0;
    public GameObject sk1;
    public GameObject sk2;

    public Transform mao0;
    public Transform mao1;
    public Transform mao2;

    BossSoundView soundManager1;
    BossHitFlashView hitFlash;
    Transform playerTarget;
    ITransientEntityService transientEntities;

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 绑定玩家目标依赖或事件监听。
    public void BindPlayerTarget(Transform target)
    {
        playerTarget = target != null ? target : throw new ArgumentNullException(nameof(target));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
        soundManager1 = GetComponentInParent<BossSoundView>();
        hitFlash = GetComponentInParent<BossHitFlashView>();
    }
    // 执行GoAtk对应的主要流程。
    public void GoAtk(TrackEntry trackEntry, Spine.Event e)
    {
        GameObject wsk = null;
        Transform wmao = null;
        string parm = "";
        if (e.ToString() == "attack")
        {
            switch (e.String)
            {
                case "atk1":
                    //soundManager1?.PlayBoss2Sound("atk1_t");
                    wsk = sk1;
                    wmao = mao1;
                    break;

                case "atk2":
                    //soundManager1?.PlayBoss2Sound("atk2_t");
                    wsk = sk2;
                    wmao = mao2;
                    parm = playerTarget != null
                        ? playerTarget.position.ToString()
                        : string.Empty;
                    break;
                case "atk0":
                    soundManager1?.TryPlay(Boss2SoundCues.HeadExitBurrow);
                    wsk = sk0;
                    wmao = mao0;
                    break;

                default:
                    break;
            }
            //print(e.String + "看看");
        }


        //print("看看：" + e.String);

        if (wsk && wmao)
        {
            if (transientEntities == null)
            {
                throw new InvalidOperationException("Boss2 attack entities were not bound by the composition root.");
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
                        ActorId.BossHead,
                        new WeaponIdentity(WeaponColor.Red, ColorTiming.Combat.WeaponType.Normal),
                        _flip,
                        parm);
                });

        }
        else
        {
            print("未正确创建技能:" + e.String);
        }

    }

    // 执行Rutu对应的主要流程。
    public void Rutu()
    {
        soundManager1?.TryPlay(Boss2SoundCues.HeadEnterBurrow);
    }

    // 响应Hit回调，并更新本对象状态。
    public void OnHit()
    {
        soundManager1?.TryPlay(Boss2SoundCues.Hit);
        hitFlash?.Play();
    }


    // 播放音效对应的动画、音频或表现。
    public void PlaySound(string atk)
    {
        switch (atk)
        {
            case "atk1":
                soundManager1?.TryPlay(Boss2SoundCues.HeadAttack1);
                break;
            case "atk2":
                soundManager1?.TryPlay(Boss2SoundCues.HeadAttack2);
                break;
            //case "atk0":
            //    soundManager1?.PlayBoss2Sound("ct_t");
            //    break;

            default:
                break;
        }
    }

}
