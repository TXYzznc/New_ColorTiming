// 文件职责：定义 Boss1动画事件Relay，承担 Boss1 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ColorTiming.Combat;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class Boss1AnimationEventRelay : MonoBehaviour, ITransientEntityConsumer
{
    public GameObject sk1;
    public GameObject sk2;
    public GameObject sk3;
    public GameObject sk3_1;
    public GameObject sk4;
    public GameObject sk5;
    public GameObject sk6;



    public GameObject mao0;

    public GameObject mao1;
    public GameObject mao2;
    public GameObject mao3;
    public GameObject mao5;

    public GameObject mao6;

    //public SkeletonRenderer skeletonRenderer1;
    //public SkeletonRenderer skeletonRenderer2;
    public SkeletonAnimation skeletonAnimation1;
    public SkeletonAnimation skeletonAnimation2;

    BossSoundView soundManager1;
    BossHitFlashView hitFlash;
    ITransientEntityService transientEntities;

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
      soundManager1 = GetComponent<BossSoundView>();
      hitFlash = GetComponent<BossHitFlashView>();
    }

    // 执行GoAtk对应的主要流程。
    public void GoAtk(TrackEntry trackEntry, Spine.Event e)
    {

        GameObject wsk = null;
        GameObject wmao = null;
        if (e.ToString() == "attack")
        {
            //soundManager1?.PlayBoss1Sound(e.String);
            switch (e.String)
            {
                case "atk1":
                    wsk = sk1;
                    wmao = mao1;
                    break;
                case "atk2":
                    wsk = sk2;
                    wmao = mao2;
                    break;
                case "atk3_1":
                    wsk = sk3;
                    wmao = mao3;
                    break;
                case "atk3_2":
                    wsk = sk3_1;
                    wmao = mao3;
                    break;
                case "atk5":
                    wsk = sk5;
                    wmao = mao5;
                    soundManager1?.TryPlayAnimationCue(e.String);
                    break;
                case "atk6":
                    wsk = sk6;
                    wmao = mao6;
                    break;

                //当atk5处理
                default:
                    print("一个未处理的动画参数：" + e.String);
                    //wsk = sk5;
                    //wmao = mao0;
                    break;
            }


            if (wsk && wmao)
            {
                if (transientEntities == null)
                {
                    throw new InvalidOperationException("Boss1 transient entities were not bound by the composition root.");
                }

                int facing = transform.localScale.x >= 0f ? 1 : -1;
                transientEntities.Spawn(
                    wsk.name,
                    wmao.transform.position,
                    wmao.transform.rotation,
                    wmao.transform,
                    instance =>
                    {
                        instance.transform.localPosition = wsk.transform.localPosition;
                        instance.transform.localRotation = wsk.transform.localRotation;
                        instance.transform.localScale = wsk.transform.localScale;

                        var skill = instance.GetComponent<Skill_base>();
                        if (skill == null)
                        {
                            Debug.LogError(
                                $"[ColorTiming.Combat][Boss1SkillSpawn] action=Configure result=missing-skill attack={e.String} entity={instance.name}",
                                instance);
                            return;
                        }

                        skill.SetSkillData(
                            ActorId.BossHead,
                            new WeaponIdentity(WeaponColor.Red, WeaponType.Normal),
                            facing,
                            string.Empty);
                        Debug.Log(
                            $"[ColorTiming.Combat][Boss1SkillSpawn] action=Configure result=success attack={e.String} entity={instance.name} attacker={ActorId.BossHead} weapon={WeaponColor.Red}",
                            instance);
                    });

            }
            else
            {
                print("未正确创建技能");
            }
        }

        if(e.ToString() == "atk_end")
        {
            //已取消收刀声
            //soundManager1?.PlayBoss1Sound("atkEnd");
        }

        if(e.ToString() == "atk_read")
        {
            soundManager1?.TryPlay(Boss1SoundCues.AttackReady);
        }



        //print("看看：" + e.String);



    }


    // 响应Hit回调，并更新本对象状态。
    public void OnHit()
    {
        soundManager1?.TryPlay(Boss1SoundCues.Hit);
        hitFlash?.Play();
    }


    // 播放音效对应的动画、音频或表现。
    public void PlaySound(string atk_)
    {
        soundManager1?.TryPlayAnimationCue(atk_);
    }

    /// <summary>
    /// 被击变色(使用Spine/Sprite/Unlit的shader)
    /// </summary>
    //void ChangeColor1()
    //{
    //    if (hittedColorTimer == 0f)
    //    {
    //        mpb.SetColor("_OverlayColor", HittedColor);
    //        meshRenderer.SetPropertyBlock(mpb);
    //    }
    //    hittedColorTimer += Time.fixedDeltaTime;
    //    if (hittedColorTimer >= changeColorTime || meshRenderer.material.color.a <= 0.1f)
    //    {
    //        hittedColorTimer = 0f;
    //        inHittedChangeColor = false;
    //        mpb.SetColor("_OverlayColor", new Color(1, 1, 1, 0));
    //        meshRenderer.SetPropertyBlock(mpb);
    //    }
    //    else
    //    {
    //        mpb.SetColor("_OverlayColor", Color.Lerp(HittedColor, new Color(HittedColor.r, HittedColor.g, HittedColor.b, 0), hittedColorTimer / changeColorTime));
    //        meshRenderer.SetPropertyBlock(mpb);
    //    }
    //}
}
