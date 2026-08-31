// 文件职责：实现战斗技能 sk_bo2_luodian 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System;
using System.Collections;
using System.Collections.Generic;
using ColorTiming.Presentation.Entities;
using ColorTiming.Configuration;
using UnityEngine;

public class sk_bo2_luodian : MonoBehaviour, IFrameworkEntityParticipant, IColorTimingSkillConfigurationConsumer
{
    Transform cseF;
    SpriteRenderer sp;
    // Update is called once per frame

    float fTime;
    float _f = 1;
    float count = 0;

    float dis;
    float releaseDelay = -1f;
    Action frameworkRelease;
    bool releasing;
    float baseInterval = 0.3f;
    float intervalStep = 0.05f;
    float minimumInterval = 0.05f;
    float pulseSpeed = 5f;
    float configuredReleaseDelay = 0.5f;

    public void BindSkillConfiguration(ColorTimingSkillTable configuration)
    {
        baseInterval = configuration.PatternA;
        intervalStep = configuration.PatternB;
        minimumInterval = configuration.PatternB;
        pulseSpeed = configuration.PatternC;
        configuredReleaseDelay = configuration.EndDelay;
    }
    // 设置CaseF，并使后续流程使用最新状态。
    public void SetCaseF(Transform _c)
    {
        cseF = _c;
        dis = Vector2.Distance(cseF.position, transform.position);
    }

    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {
        if (releaseDelay > 0f)
        {
            releaseDelay -= Time.deltaTime;
            if (releaseDelay <= 0f)
            {
                ReleaseEntity();
                return;
            }
        }

        if (sp == null)
        {
            sp = GetComponent<SpriteRenderer>();
        }
        else
        {
            float tf = baseInterval;
            tf -= count * intervalStep;
            tf = tf > minimumInterval ? tf : minimumInterval;

            if (fTime > tf)
            {
                _f = -1;

            }
            else if (fTime < 0)
            {
                _f = 1;
                count++;

            }

            //print(tf);
            fTime += Time.deltaTime * _f * tf * pulseSpeed;

            float fg = Mathf.Lerp(0, 1, fTime / tf);

            sp.color = new Color(1, 1, 1, fg);

        }



        //if (sp == null)
        //{
        //    sp = GetComponent<SpriteRenderer>();
        //}
        //else
        //{
        //    float tf = 0.1f;
        //    //tf -= count * 0.05f;
        //    //tf = tf > 0.05f ? tf : 0.05f;

        //    if (cseF != null)
        //    {
        //        float _dis = Vector2.Distance(cseF.position, transform.position);


        //        tf = Mathf.Lerp(0.05f, 0.3f, _dis / dis);
        //        //print(tf);
        //    }

        //    if (tf > 0.05f)
        //    {
        //        if (fTime > tf)
        //        {
        //            _f = -1;
        //        }
        //        else if (fTime < 0)
        //        {
        //            _f = 1;
        //            count++;
        //        }

        //        fTime += Time.deltaTime * _f * (1 - tf) * 3;

        //        //print(fTime);

        //        float fg = Mathf.Lerp(0, 1, fTime / tf);

        //        sp.color = new Color(1, 1, 1, fg);
        //    }
        //    else
        //    {
        //        sp.color = new Color(1, 1, 1, 1);
        //    }
        //}
    }


    // 设置Wait结束，并使后续流程使用最新状态。
    public void SetWaitEnd()
    {
        releaseDelay = configuredReleaseDelay;
    }

    // 释放实体及其临时资源。
    public void ReleaseEntity()
    {
        if (releasing)
        {
            return;
        }
        releasing = true;
        if (frameworkRelease != null)
        {
            frameworkRelease.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
    public void OnFrameworkEntitySpawned()
    {
        fTime = 0f;
        _f = 1f;
        count = 0f;
        releaseDelay = -1f;
        releasing = false;
        sp = sp != null ? sp : GetComponent<SpriteRenderer>();
    }

    // 响应Framework实体Despawned回调，并更新本对象状态。
    public void OnFrameworkEntityDespawned()
    {
        cseF = null;
        releaseDelay = -1f;
        releasing = false;
    }

}
