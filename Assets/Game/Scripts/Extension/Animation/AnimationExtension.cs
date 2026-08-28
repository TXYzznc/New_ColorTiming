// 文件职责：提供 动画 相关的通用扩展方法。
// 所属模块：Extension / Animation。

using System;
using DG.Tweening;
using UnityEngine;

public static class AnimationExtension
{
    // 播放Backward对应的动画、音频或表现。
    public static void PlayBackward(this Animation animation, string name, Action onComplete = null)
    {
        var animState = animation[name];
        float duration = animState.length - 0.001f;
        animState.time = duration;
        animation.Play(name);
        var motionHandle = DOVirtual.Float(animState.length, 0, duration, v =>
        {
            animState.time = v;
        }).SetUpdate(true).SetEase(Ease.Linear).SetTarget(animation);
        if (onComplete != null) motionHandle.onComplete = () => { onComplete.Invoke(); };
    }

    // 播放Forward对应的动画、音频或表现。
    public static void PlayForward(this Animation animation, string name, Action onComplete = null)
    {
        var animState = animation[name];
        float duration = animState.length - 0.001f;
        animState.time = 0;
        animation.Play(name);
        var motionHandle = DOVirtual.Float(0, animState.length, duration, v =>
        {
            animState.time = v;
        }).SetUpdate(true).SetEase(Ease.Linear).SetTarget(animation);
        if (onComplete != null) motionHandle.onComplete = () => { onComplete.Invoke(); };
    }
}
