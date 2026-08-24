
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;


public class Boss1SoundManager : MonoBehaviour, IColorTimingSoundConsumer
{
    IColorTimingSoundService soundService;

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

    public void PlayBoss1Sound(string tName)
    {
        AudioClip _ac = null;

        switch (tName)
        {
            case "hit":_ac = hit;
                break;

            case "atk1":
                _ac = atk1;
                break;
            case "atk2":
                _ac = atk2;
                break;
            case "atk3_1":
                _ac= atk3;
                break;
            case "atk4":
                _ac = atk4;
                break;
            case "atk5":
                _ac = atk5;

                break;
            case "atk6": _ac = atk6; break;
            case "atkEnd":_ac = atkEnd; break;
            case "atkReady":_ac = atkReady; break;
            default:
                break;
        }

        if(_ac == null) return;

        soundService?.Play(_ac, ColorTimingSoundChannel.Boss, transform.position);
        //print("已播放音效：" + _ac.name);
    }

}
