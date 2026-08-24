using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartVido : MonoBehaviour
{
    VideoPlayer player;
    Coroutine switchRoutine;
    RenderTexture outputTexture;
    RawImage videoDisplay;

    public VideoPlayer loop2;
    void Awake()
    {
        player = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (player == null) player = GetComponent<VideoPlayer>();
        if (player != null) player.loopPointReached += PlayEnd;
        EnsureVideoOutput();
    }

    void OnDisable()
    {
        if (player != null) player.loopPointReached -= PlayEnd;
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
    }

    private void PlayEnd(VideoPlayer source)
    {
        if (switchRoutine == null)
        {
            switchRoutine = StartCoroutine(SwitchToLoop());
        }
    }

    IEnumerator SwitchToLoop()
    {
        if (loop2 != null)
        {
            loop2.gameObject.SetActive(true);
            loop2.isLooping = true;
            loop2.Play();
        }
        yield return new WaitForSecondsRealtime(0.1f);
        switchRoutine = null;
        gameObject.SetActive(false);
    }

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
        if (player != null)
        {
            player.Stop();
            player.time = 0d;
            player.Play();
        }
    }

    public void StopSequence()
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
        player?.Stop();
        loop2?.Stop();
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
                "VideoOutput",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            var displayTransform = (RectTransform)displayObject.transform;
            displayTransform.SetParent(transform.parent, false);
            displayTransform.SetSiblingIndex(1);
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

    private void OnDestroy()
    {
        if (outputTexture == null)
        {
            return;
        }

        outputTexture.Release();
        Destroy(outputTexture);
        outputTexture = null;
    }
}
