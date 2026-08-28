using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Presentation.Audio;

public enum PlayerSoundCue
{
    PickupWeapon,
    DropWeapon,
}

public class PlayerSoundView : MonoBehaviour, IColorTimingSoundConsumer
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

    public void Play(PlayerSoundCue cue)
    {
        AudioClip clip;
        switch (cue)
        {
            case PlayerSoundCue.PickupWeapon:
                clip = pickUPWeapom;
                break;
            case PlayerSoundCue.DropWeapon:
                clip = disWeapon;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }

        PlayAuido(clip);
    }

    public void PlayAuido_Random(AudioClip[] audioClips)
    {

    }

    public void PlayAuido_Random(List<AudioClip> audioClips)
    {

    }
}
