// 文件职责：实现战斗技能 HitFX_Base 的运行时表现和回收行为。
// 所属模块：ColorTiming / Presentation / Combat / Skills。

using System;
using ColorTiming.Presentation.Entities;
using UnityEngine;

public class HitFX_Base : MonoBehaviour, IFrameworkEntityParticipant
{
    Action frameworkRelease;

    // 响应FX结束回调，并更新本对象状态。
    void OnFXEnd()
    {
        if (frameworkRelease != null)
        {
            frameworkRelease.Invoke();
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }
    }

    // 绑定FrameworkRelease依赖或事件监听。
    public void BindFrameworkRelease(Action release)
    {
        frameworkRelease = release;
    }

    // 响应Framework实体Spawned回调，并更新本对象状态。
    public void OnFrameworkEntitySpawned() { }
    public void OnFrameworkEntityDespawned() { }
}
