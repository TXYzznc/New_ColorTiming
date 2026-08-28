// 文件职责：定义 Boss1动画事件Relay，承担 Boss1 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

    public MeshRenderer meshRenderer1;
    public MeshRenderer meshRenderer2;

    //public SkeletonRenderer skeletonRenderer1;
    //public SkeletonRenderer skeletonRenderer2;
    public SkeletonAnimation skeletonAnimation1;
    public SkeletonAnimation skeletonAnimation2;

    Boss1SoundView soundManager1;
    ITransientEntityService transientEntities;

    // 绑定TransientEntities依赖或事件监听。
    public void BindTransientEntities(ITransientEntityService entities)
    {
        transientEntities = entities ?? throw new ArgumentNullException(nameof(entities));
    }

    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {
      soundManager1 = GetComponent<Boss1SoundView>();
    }
    bool flip;
    float _it;
    float lerpSpeed = 10;
    float _showTime = -1;
    // 逐帧推进需要实时刷新的业务或表现状态。
    private void Update()
    {

        if (_showTime > 0)
        {
            _showTime -= Time.deltaTime;
            //print(_showTime);

            ShowHit();

        }
        else
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            //mpb.SetColor("_Black", Color.black);
            mpb.SetFloat("_FillPhase", 0);

            meshRenderer1.SetPropertyBlock(mpb);
            meshRenderer2.SetPropertyBlock(mpb);
            //skeletonAnimation1?.skeleton?.SetColor(Color.white);
            //skeletonAnimation2?.skeleton?.SetColor(Color.white);
            //print("sssssssssssss");
        }


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
            soundManager1?.Play(Boss1SoundCue.AttackReady);
        }



        //print("看看：" + e.String);



    }


    // 响应Hit回调，并更新本对象状态。
    public void OnHit()
    {
        soundManager1?.Play(Boss1SoundCue.Hit);
        _showTime = 0.2f;
    }


    // 显示Hit并同步当前数据。
    void ShowHit()
    {
        float _sp = flip ? -1 : 1;
        _it += (Time.deltaTime * lerpSpeed * _sp);

        Color _c = new Color(_it, _it, _it, _it);

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        //mpb.SetColor("_Black", _c);
        mpb.SetFloat("_FillPhase", _it);
        meshRenderer1.SetPropertyBlock(mpb);
        meshRenderer2.SetPropertyBlock(mpb);
        // meshRenderer1.material.SetColor("Dark Color", _c);


        //print(_c);

        //print(_it);
        //skeletonAnimation1?.skeleton?.SetColor(_c);
        //skeletonAnimation2?.skeleton?.SetColor(_c);


        if (flip)
        {
            //检查
            if (_it < 0) flip = false;
        }
        else
        {
            if (_it > 1) flip = true;
        }
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
