// 文件职责：实现战斗技能 Sk_Bo2_dundi 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class Sk_Bo2_dundi : MonoBehaviour, IFrameworkEntityParticipant
{
    const string animName1 = "animation";
    const string animName2 = "animation2";
    const string animName3 = "animation3";
    SkeletonAnimation _s;
    TrackEntry currentEntry;
    Action frameworkRelease;

    public Boss2ActorView boss2_Controller;
    //public GameObject ins;
    // Start is called before the first frame update
    // 在首帧启动依赖就绪后的业务或表现流程。
    void Start()
    {
        if (currentEntry == null)
        {
            InitializeAnimation();
        }

        //已放弃播放2循环
        //PlayAnim(animName2);
        //entry.Event += AnimEvent;
    }

    // 播放Anim对应的动画、音频或表现。
    void PlayAnim(string anim)
    {
        currentEntry = _s.AnimationState.SetAnimation(0, anim, false);

        currentEntry.Complete += Entry_Complete;
    }

    private void Entry_Complete(TrackEntry trackEntry)
    {
        trackEntry.Complete -= Entry_Complete;

        currentEntry = null;
        if (frameworkRelease != null)
        {
            frameworkRelease.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }


        //已修改为仅播放1
        //if (trackEntry.Animation.Name == animName2)
        //{
        //    PlayAnim(animName1);
        //    //boss2_Controller?.CreateDundi();
        //}
        //else if (trackEntry.Animation.Name == animName1)
        //{

        //    PlayAnim(animName3);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}

    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
    public void OnFrameworkEntitySpawned()
    {
        InitializeAnimation();
    }

    // 响应Framework实体Despawned回调，并更新本对象状态。
    public void OnFrameworkEntityDespawned()
    {
        if (currentEntry != null)
        {
            currentEntry.Complete -= Entry_Complete;
            currentEntry = null;
        }
    }

    // 初始化动画及其依赖关系。
    private void InitializeAnimation()
    {
        _s = _s != null ? _s : GetComponent<SkeletonAnimation>();
        PlayAnim(animName1);
    }
}
