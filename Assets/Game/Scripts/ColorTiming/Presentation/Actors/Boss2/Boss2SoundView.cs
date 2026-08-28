using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public enum Boss2SoundCue
{
    Hit,
    HeadEnterBurrow,
    HeadExitBurrow,
    TailEnterBurrow,
    TailExitBurrow,
    HeadAttack1,
    HeadAttack2,
    TailAttack1,
    TailAttack2,
}

public class Boss2SoundView : MonoBehaviour, IColorTimingSoundConsumer
{
    IColorTimingSoundService soundService;

    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }
    public AudioClip hit;
    public AudioClip rt_tou;
    public AudioClip ct_tou;
    public AudioClip rt_wei;
    public AudioClip ct_wei;

    public AudioClip atk1_tou;
    public AudioClip atk2_tou;

    public AudioClip atk1_wei;
    public AudioClip atk2_wei;


    public bool TryPlayAnimationCue(string key)
    {
        switch (key)
        {
            case "hit": Play(Boss2SoundCue.Hit); return true;
            case "atk1_t": Play(Boss2SoundCue.HeadAttack1); return true;
            case "atk2_t": Play(Boss2SoundCue.HeadAttack2); return true;
            case "atk1_w": Play(Boss2SoundCue.TailAttack1); return true;
            case "atk2_w": Play(Boss2SoundCue.TailAttack2); return true;
            case "rt_t": Play(Boss2SoundCue.HeadEnterBurrow); return true;
            case "ct_t": Play(Boss2SoundCue.HeadExitBurrow); return true;
            case "rt_w": Play(Boss2SoundCue.TailEnterBurrow); return true;
            case "ct_w": Play(Boss2SoundCue.TailExitBurrow); return true;
            default: return false;
        }
    }

    public void Play(Boss2SoundCue cue)
    {
        AudioClip clip;
        switch (cue)
        {
            case Boss2SoundCue.Hit: clip = hit; break;
            case Boss2SoundCue.HeadEnterBurrow: clip = rt_tou; break;
            case Boss2SoundCue.HeadExitBurrow: clip = ct_tou; break;
            case Boss2SoundCue.TailEnterBurrow: clip = rt_wei; break;
            case Boss2SoundCue.TailExitBurrow: clip = ct_wei; break;
            case Boss2SoundCue.HeadAttack1: clip = atk1_tou; break;
            case Boss2SoundCue.HeadAttack2: clip = atk2_tou; break;
            case Boss2SoundCue.TailAttack1: clip = atk1_wei; break;
            case Boss2SoundCue.TailAttack2: clip = atk2_wei; break;
            default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }

        if (clip != null) soundService?.Play(clip, ColorTimingSoundChannel.Boss, transform.position);
    }
}
