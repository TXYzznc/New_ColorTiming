// 文件职责：负责 玩家音效 的场景或界面表现。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using ColorTiming.Presentation.Audio;

public enum PlayerSoundCue
{
    PickupWeapon,
    DropWeapon,
}

public class PlayerSoundView : MonoBehaviour, IColorTimingSoundConsumer, IColorTimingConfigurationConsumer
{
    IColorTimingSoundService soundService;
    readonly List<string> dashCues = new List<string>();
    readonly List<string> moveCues = new List<string>();
    readonly List<string> moveOverrideCues = new List<string>();

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService service)
    {
        soundService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public List<string> moveCase = new List<string>();
    public int MoveCueCount => moveCues.Count;
    public int MoveOverrideCueCount => moveOverrideCues.Count;

    /// <summary>按场景读取玩家音效 Cue，资源引用只存在于 DataTable。</summary>
    public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        var sceneKey = sceneId == ColorTimingSceneId.Boss1 ? "boss1" : "boss2";
        CopyCueIds(configuration.GetSoundCues($"player.{sceneKey}.dash."), dashCues);
        CopyCueIds(configuration.GetSoundCues($"player.{sceneKey}.move."), moveCues);
        CopyCueIds(configuration.GetSoundCues($"player.{sceneKey}.move-override."), moveOverrideCues);
        if (dashCues.Count == 0 || moveCues.Count == 0)
            throw new InvalidOperationException($"Player sound configuration for '{sceneKey}' is incomplete.");
        moveCase.Clear();
    }

    private static void CopyCueIds(IReadOnlyList<ColorTimingSoundCueTable> source, ICollection<string> destination)
    {
        destination.Clear();
        for (var i = 0; i < source.Count; i++) destination.Add(source[i].CueId);
    }
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
        IReadOnlyList<string> cues = Array.Empty<string>();

        switch (randomName)
        {
            case "dash":
                cues = dashCues;
                break;
            case "move":
                cues = moveCase.Count > 0 && moveOverrideCues.Count > 0 ? moveOverrideCues : moveCues;
                break;
            default:
                break;
        }

        if (cues.Count > 0) soundService?.PlayCue(cues[UnityEngine.Random.Range(0, cues.Count)], transform.position);
    }

    // 启动当前配置的动画、音频或其他表现。
    public void Play(PlayerSoundCue cue)
    {
        string cueId;
        switch (cue)
        {
            case PlayerSoundCue.PickupWeapon:
                cueId = "player.pickup";
                break;
            case PlayerSoundCue.DropWeapon:
                cueId = "player.drop";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
        }

        soundService?.PlayCue(cueId, transform.position);
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
