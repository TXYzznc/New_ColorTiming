using System;
using ColorTiming.Presentation.Audio;
using UnityEngine;

/// <summary>Serialized StartMenu adapter that routes UI sounds through GF.Sound.</summary>
public class UiSoundView : MonoBehaviour, IUiSoundSink, IColorTimingSoundConsumer
{
    public AudioClip click;
    public AudioClip hover;
    IColorTimingSoundService soundService;

    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void PlayClick()
    {
        Play(click);
    }

    public void PlayHover()
    {
        Play(hover);
    }

    // Preserves existing UnityEvent compatibility.
    public void PlayBtnSound(bool isClick)
    {
        if (isClick) PlayClick();
        else PlayHover();
    }

    void Play(AudioClip clip)
    {
        soundService?.Play(clip, ColorTimingSoundChannel.UI, Vector3.zero);
    }
}
