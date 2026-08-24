using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public class Boss2SoundManager : MonoBehaviour, IColorTimingSoundConsumer
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


    public void PlayBoss2Sound(string tName)
    {
        AudioClip _ac = null;

        switch (tName)
        {
            case "hit":_ac = hit; break;
            case "atk1_t":_ac = atk1_tou;break;
            case "atk2_t":_ac=atk2_tou;break;
            case "atk1_w":_ac=atk1_wei;break;
            case "atk2_w":_ac = atk2_wei;break;
            case "rt_t":_ac = rt_tou;break;
            case "ct_t":_ac = ct_tou;break;
            case "rt_w":_ac = rt_wei;break;
            case "ct_w":_ac = ct_wei;break;

            default:
                break;
        }

        if (_ac == null) return; ;
        soundService?.Play(_ac, ColorTimingSoundChannel.Boss, transform.position);
    }
}
