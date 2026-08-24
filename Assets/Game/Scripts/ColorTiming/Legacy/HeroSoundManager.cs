using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public class HeroSoundManager : MonoBehaviour, IColorTimingSoundConsumer
{
    IColorTimingSoundService soundService;

    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public List<AudioClip> rDashAuido;
    public List<AudioClip> rMoveAudio;
    public List<AudioClip> rMove_Overwrite_Audio;

    public AudioClip pickUPWeapom;
    public AudioClip disWeapon;

    public List<string> moveCase = new List<string>();
    public void AddOverwriteMoveCase(bool add,string name)
    {
        if (add)
        {
            moveCase.Add(name);
        }
        else
        {
            moveCase.Remove(name);
        }

    }

    public void PlayAuido(AudioClip audioClip)
    {
        soundService?.Play(audioClip, ColorTimingSoundChannel.Player, transform.position);
    }

    public void PlayAuido_Random(string randomName)
    {
        List<AudioClip> audioClips = new List<AudioClip>();

        switch (randomName)
        {
            case "dash":
                audioClips = rDashAuido;
                break;
            case "move":
                audioClips = moveCase.Count > 0 ?rMove_Overwrite_Audio : rMoveAudio ;
                break;                    
            default:
                break;
        }
        
        audioClips = FunctionLibrary.RandomSort(audioClips);
        if (audioClips.Count > 0)
        {
            PlayAuido(audioClips[0]);
        }
      

    }

    public void PlayAudio_Name(string name)
    {
        AudioClip _ac = null;
        switch (name)
        {
            case "pickupWapon": _ac = pickUPWeapom; 
                break;
            case "disWeapon":_ac = disWeapon;
                break;
            default:
                break;
        }

        PlayAuido(_ac);
    }

    public void PlayAuido_Random(AudioClip[] audioClips)
    {

    }

    public void PlayAuido_Random(List<AudioClip> audioClips)
    {

    }
}
