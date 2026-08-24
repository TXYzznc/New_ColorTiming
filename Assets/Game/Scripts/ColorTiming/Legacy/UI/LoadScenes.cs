using System;
using ColorTiming.Bootstrap.Flow;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Persistent legacy loading view. Scene ownership belongs to the framework scene flow;
/// this component only renders monotonic progress and unscaled fades.
/// </summary>
public class LoadScenes : MonoBehaviour, IColorTimingSceneFlowConsumer
{
    static LoadScenes persistentView;

    public GameObject lodingCanvas;
    public GameObject jindu;
    public Slider progress;
    public Image fead;

    IColorTimingSceneFlow sceneFlow;
    bool fadingOut;
    float fadeAlpha;
    const float FadeSpeed = 2f;

    private void Awake()
    {
        if (persistentView != null && persistentView != this)
        {
            Destroy(gameObject);
            return;
        }

        persistentView = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        if (sceneFlow == flow)
        {
            return;
        }
        Unsubscribe();
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
        sceneFlow.TransitionStarted += OnTransitionStarted;
        sceneFlow.TransitionProgress += OnTransitionProgress;
        sceneFlow.SceneChanged += OnSceneChanged;
        sceneFlow.TransitionFailed += OnTransitionFailed;
    }

    private void Start()
    {
        SetLoadingVisible(false);
    }

    private void Update()
    {
        if (!fadingOut || lodingCanvas == null || !lodingCanvas.activeSelf)
        {
            return;
        }

        fadeAlpha = Mathf.Max(0f, fadeAlpha - Time.unscaledDeltaTime * FadeSpeed);
        SetFadeAlpha(fadeAlpha);
        if (fadeAlpha <= 0f)
        {
            fadingOut = false;
            SetLoadingVisible(false);
        }
    }

    // Preserves any serialized UnityEvent that called the old loader directly.
    public void LoadSceneSync(string sceneName)
    {
        if (!Enum.TryParse(sceneName, true, out ColorTimingSceneId scene))
        {
            Debug.LogError($"Unknown ColorTiming scene '{sceneName}'.", this);
            return;
        }
        sceneFlow?.TryLoad(scene);
    }

    void OnTransitionStarted(ColorTimingSceneId scene)
    {
        fadingOut = false;
        fadeAlpha = 1f;
        if (progress != null) progress.value = 0f;
        if (jindu != null) jindu.SetActive(true);
        SetFadeAlpha(1f);
        SetLoadingVisible(true);
    }

    void OnTransitionProgress(float value)
    {
        if (progress != null)
        {
            progress.value = Mathf.Max(progress.value, Mathf.Clamp01(value));
        }
    }

    void OnSceneChanged(ColorTimingSceneId scene)
    {
        if (progress != null) progress.value = 1f;
        if (jindu != null) jindu.SetActive(false);
        fadingOut = true;
    }

    void OnTransitionFailed(ColorTimingSceneId scene, string error)
    {
        fadingOut = false;
        SetLoadingVisible(false);
    }

    void SetLoadingVisible(bool visible)
    {
        if (lodingCanvas != null) lodingCanvas.SetActive(visible);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fead != null) fead.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
    }

    void Unsubscribe()
    {
        if (sceneFlow == null) return;
        sceneFlow.TransitionStarted -= OnTransitionStarted;
        sceneFlow.TransitionProgress -= OnTransitionProgress;
        sceneFlow.SceneChanged -= OnSceneChanged;
        sceneFlow.TransitionFailed -= OnTransitionFailed;
        sceneFlow = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (persistentView == this) persistentView = null;
    }
}
