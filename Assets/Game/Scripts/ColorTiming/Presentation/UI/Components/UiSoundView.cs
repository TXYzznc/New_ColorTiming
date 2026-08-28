// 文件职责：负责 UI音效 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System;
using ColorTiming.Presentation.Audio;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Components
{
/// <summary>Serialized StartMenu adapter that routes UI sounds through GF.Sound.</summary>
public class UiSoundView : MonoBehaviour, IUiSoundSink, IColorTimingSoundConsumer
{
    public AudioClip click;
    public AudioClip hover;
    IColorTimingSoundService soundService;

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    // 播放Click对应的动画、音频或表现。
    public void PlayClick()
    {
        Play(click);
    }

    // 播放Hover对应的动画、音频或表现。
    public void PlayHover()
    {
        Play(hover);
    }

    // Preserves existing UnityEvent compatibility.
    // 播放Btn音效对应的动画、音频或表现。
    public void PlayBtnSound(bool isClick)
    {
        if (isClick) PlayClick();
        // 播放Hover对应的动画、音频或表现。
        else PlayHover();
    }

    // 启动当前配置的动画、音频或其他表现。
    void Play(AudioClip clip)
    {
        soundService?.Play(clip, ColorTimingSoundChannel.UI, Vector3.zero);
    }
}
}
