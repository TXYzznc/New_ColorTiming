// 文件职责：负责 Boss1音效 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Boss1。

using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public enum Boss1SoundCue
{
    Hit,
    AttackReady,
    AttackEnd,
    Attack1,
    Attack2,
    Attack3,
    Attack4,
    Attack5,
    Attack6,
}


public class Boss1SoundView : MonoBehaviour, IColorTimingSoundConsumer
{
    IColorTimingSoundService soundService;

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }
    public AudioClip hit;
    public AudioClip atkReady;

    public AudioClip atkEnd;

    public AudioClip atk1;
    public AudioClip atk2;
    public AudioClip atk3;
    public AudioClip atk4;
    public AudioClip atk5;
    public AudioClip atk6;

    // 尝试Play动画Cue，并通过返回值报告是否成功。
    public bool TryPlayAnimationCue(string key)
    {
        switch (key)
        {
            // 启动当前配置的动画、音频或其他表现。
            case "hit": Play(Boss1SoundCue.Hit); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atkReady": Play(Boss1SoundCue.AttackReady); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atkEnd": Play(Boss1SoundCue.AttackEnd); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk1": Play(Boss1SoundCue.Attack1); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk2": Play(Boss1SoundCue.Attack2); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk3_1": Play(Boss1SoundCue.Attack3); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk4": Play(Boss1SoundCue.Attack4); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk5": Play(Boss1SoundCue.Attack5); return true;
            // 启动当前配置的动画、音频或其他表现。
            case "atk6": Play(Boss1SoundCue.Attack6); return true;
            default: return false;
        }
    }

    // 启动当前配置的动画、音频或其他表现。
    public void Play(Boss1SoundCue cue)
    {
        AudioClip clip;
        switch (cue)
        {
            case Boss1SoundCue.Hit: clip = hit; break;
            case Boss1SoundCue.AttackReady: clip = atkReady; break;
            case Boss1SoundCue.AttackEnd: clip = atkEnd; break;
            case Boss1SoundCue.Attack1: clip = atk1; break;
            case Boss1SoundCue.Attack2: clip = atk2; break;
            case Boss1SoundCue.Attack3: clip = atk3; break;
            case Boss1SoundCue.Attack4: clip = atk4; break;
            case Boss1SoundCue.Attack5: clip = atk5; break;
            case Boss1SoundCue.Attack6: clip = atk6; break;
            default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }

        if (clip != null) soundService?.Play(clip, ColorTimingSoundChannel.Boss, transform.position);
    }

}
