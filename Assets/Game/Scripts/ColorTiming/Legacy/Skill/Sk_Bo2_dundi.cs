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

    public Boss2_Controller boss2_Controller;
    //public GameObject ins;
    // Start is called before the first frame update
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

    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    public void OnFrameworkEntitySpawned()
    {
        InitializeAnimation();
    }

    public void OnFrameworkEntityDespawned()
    {
        if (currentEntry != null)
        {
            currentEntry.Complete -= Entry_Complete;
            currentEntry = null;
        }
    }

    private void InitializeAnimation()
    {
        _s = _s != null ? _s : GetComponent<SkeletonAnimation>();
        PlayAnim(animName1);
    }
}
