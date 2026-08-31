// 文件职责：定义 Main菜单Intro序列，承担 Components 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / UI / Components。

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ColorTiming.Presentation.UI.Components
{
public class MainMenuIntroSequence : MonoBehaviour
{
    float loopPrepareTimeout = 10f;

    public void Configure(float timeout) => loopPrepareTimeout = Mathf.Max(0.1f, timeout);

    VideoPlayer player;
    Coroutine switchRoutine;
    Coroutine introPrepareRoutine;
    RenderTexture outputTexture;
    RawImage videoDisplay;

    public VideoPlayer loop2;
    public RawImage VideoDisplay => videoDisplay;
    // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
    void Awake()
    {
        player = GetComponent<VideoPlayer>();
        if (player != null)
        {
            player.playOnAwake = false;
        }
    }

    // 组件启用时注册监听并同步当前状态。
    void OnEnable()
    {
        if (player == null) player = GetComponent<VideoPlayer>();
        if (player != null) player.loopPointReached += PlayEnd;
        EnsureVideoOutput();
    }

    // 组件停用时解除监听并停止临时流程。
    void OnDisable()
    {
        if (player != null) player.loopPointReached -= PlayEnd;
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
        if (introPrepareRoutine != null)
        {
            StopCoroutine(introPrepareRoutine);
            introPrepareRoutine = null;
        }
    }

    // 播放结束对应的动画、音频或表现。
    private void PlayEnd(VideoPlayer source)
    {
        if (switchRoutine == null)
        {
            switchRoutine = StartCoroutine(SwitchToLoop());
        }
    }

    IEnumerator SwitchToLoop()
    {
        if (loop2 == null)
        {
            switchRoutine = null;
            yield break;
        }

        loop2.gameObject.SetActive(true);
        loop2.isLooping = true;
        loop2.Stop();
        loop2.time = 0d;
        loop2.Prepare();

        var prepareDeadline = Time.realtimeSinceStartup + loopPrepareTimeout;
        while (!loop2.isPrepared && Time.realtimeSinceStartup < prepareDeadline)
        {
            yield return null;
        }

        if (!loop2.isPrepared)
        {
            Debug.LogError("StartMenu loop video did not prepare; keeping the intro output active.", loop2);
            loop2.gameObject.SetActive(false);
            switchRoutine = null;
            yield break;
        }

        loop2.Play();
        var playbackDeadline = Time.realtimeSinceStartup + loopPrepareTimeout;
        while ((!loop2.isPlaying || loop2.texture == null) && Time.realtimeSinceStartup < playbackDeadline)
        {
            yield return null;
        }

        if (!loop2.isPlaying || loop2.texture == null)
        {
            Debug.LogError("StartMenu loop video prepared but did not produce a playable texture; keeping the intro output active.", loop2);
            loop2.gameObject.SetActive(false);
            switchRoutine = null;
            yield break;
        }

        // Both players share one RenderTexture. Wait until the loop player owns a
        // rendered frame before disabling the intro player, otherwise disabling
        // the intro can clear the shared target and leave the settings page black.
        if (global::UnityEngine.Application.isBatchMode)
        {
            // WaitForEndOfFrame is not advanced by Unity's batch player loop.
            yield return null;
        }
        else
        {
            yield return new WaitForEndOfFrame();
        }
        player.Stop();
        switchRoutine = null;
        gameObject.SetActive(false);
    }

    // 执行Restart序列对应的主要流程。
    public void RestartSequence()
    {
        if (player == null) player = GetComponent<VideoPlayer>();
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }

        if (loop2 != null)
        {
            loop2.Stop();
            loop2.time = 0d;
            loop2.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        EnsureVideoOutput();
        if (player != null && introPrepareRoutine == null)
        {
            introPrepareRoutine = StartCoroutine(PrepareAndPlayIntro());
        }
    }

    // 停止序列并清理临时播放状态。
    public void StopSequence()
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
        player?.Stop();
        loop2?.Stop();
        if (introPrepareRoutine != null)
        {
            StopCoroutine(introPrepareRoutine);
            introPrepareRoutine = null;
        }
        ReleaseVideoOutput();
    }

    IEnumerator PrepareAndPlayIntro()
    {
        player.Stop();
        player.time = 0d;
        player.Prepare();
        var prepareDeadline = Time.realtimeSinceStartup + loopPrepareTimeout;
        while (!player.isPrepared && Time.realtimeSinceStartup < prepareDeadline)
        {
            yield return null;
        }

        if (!player.isPrepared)
        {
            Debug.LogError("[ColorTiming.Video] action=Intro.Prepare result=Timeout; starting playback as fallback.", player);
        }
        else
        {
            Debug.Log(
                $"[ColorTiming.Video] action=Intro.Prepare result=Success frame={Time.frameCount} realtime={Time.realtimeSinceStartup:0.000}");
        }

        player.Play();
        introPrepareRoutine = null;
    }

    private void EnsureVideoOutput()
    {
        if (player == null)
        {
            return;
        }

        if (outputTexture == null)
        {
            var width = player.clip != null && player.clip.width > 0 ? (int)player.clip.width : 1920;
            var height = player.clip != null && player.clip.height > 0 ? (int)player.clip.height : 1080;
            outputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = "ColorTiming StartMenu Video",
                useMipMap = false,
                autoGenerateMips = false
            };
            outputTexture.Create();
        }

        if (videoDisplay == null)
        {
            var displayObject = new GameObject(
                "VideoOutput (Clone)",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            var displayTransform = (RectTransform)displayObject.transform;
            displayTransform.SetParent(transform.parent, false);
            displayTransform.SetSiblingIndex(1);
            displayObject.layer = gameObject.layer;
            displayTransform.anchorMin = Vector2.zero;
            displayTransform.anchorMax = Vector2.one;
            displayTransform.offsetMin = Vector2.zero;
            displayTransform.offsetMax = Vector2.zero;
            videoDisplay = displayObject.GetComponent<RawImage>();
            videoDisplay.raycastTarget = false;
        }

        videoDisplay.texture = outputTexture;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = outputTexture;
        if (loop2 != null)
        {
            loop2.renderMode = VideoRenderMode.RenderTexture;
            loop2.targetTexture = outputTexture;
        }
    }

    // 组件销毁时释放订阅、句柄和运行时资源。
    private void OnDestroy()
    {
        ReleaseVideoOutput();
    }

    // GF.UI 会缓存已关闭的表单，因此不能等待 OnDestroy 才释放视频输出纹理。
    // 释放前先解除 VideoPlayer 与 RawImage 的引用，确保返回主菜单时可以安全重建。
    private void ReleaseVideoOutput()
    {
        if (outputTexture == null)
        {
            return;
        }

        if (player != null && player.targetTexture == outputTexture)
        {
            player.targetTexture = null;
        }
        if (loop2 != null && loop2.targetTexture == outputTexture)
        {
            loop2.targetTexture = null;
        }
        if (videoDisplay != null && videoDisplay.texture == outputTexture)
        {
            videoDisplay.texture = null;
        }

        outputTexture.Release();
        Destroy(outputTexture);
        outputTexture = null;
    }
}
}
