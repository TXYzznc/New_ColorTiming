// 文件职责：负责 玩家音效 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

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

    // 绑定音效Service依赖或事件监听。
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
    // 添加Overwrite移动输入Case并维护相关集合状态。
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

    // 播放Auido对应的动画、音频或表现。
    public void PlayAuido(AudioClip audioClip)
    {
        soundService?.Play(audioClip, ColorTimingSoundChannel.Player, transform.position);
    }

    // 播放Auido随机源对应的动画、音频或表现。
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

    // 启动当前配置的动画、音频或其他表现。
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

    // 播放Auido随机源对应的动画、音频或表现。
    public void PlayAuido_Random(AudioClip[] audioClips)
    {

    }

    // 播放Auido随机源对应的动画、音频或表现。
    public void PlayAuido_Random(List<AudioClip> audioClips)
    {

    }
}
