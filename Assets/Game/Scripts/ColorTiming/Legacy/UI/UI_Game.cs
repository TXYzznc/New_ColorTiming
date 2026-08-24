using System;
using System.Collections;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Serialized scene adapter for battle results. Domain code reports a result through
/// IBattleResultSink; framework-owned services control time and scene transitions.
/// </summary>
public class UI_Game : MonoBehaviour, IColorTimingSceneFlowConsumer, IColorTimingUiConsumer,
    IBattleResultSink
{
    public Image ui_shengli;
    public GameObject ui_shibai;

    IColorTimingSceneFlow sceneFlow;
    IColorTimingUiService uiService;
    Coroutine pendingTransition;
    float fade = 10f;
    float fadeColor;

    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    public void BindUiService(IColorTimingUiService service)
    {
        uiService = service ?? throw new ArgumentNullException(nameof(service));
    }

    private void Update()
    {
        if (fade < 1f && ui_shengli != null)
        {
            fade = Mathf.Min(1f, fade + Time.unscaledDeltaTime);
            ui_shengli.color = new Color(fadeColor, fadeColor, fadeColor, fade);
        }

    }

    public void Show(BattlePresentationResult result)
    {
        switch (result)
        {
            case BattlePresentationResult.Boss1Defeated:
                if (pendingTransition == null)
                {
                    pendingTransition = StartCoroutine(LoadBoss2AfterDelay());
                }
                break;
            case BattlePresentationResult.FinalVictory:
                uiService?.ShowBattleResult(result);
                break;
            case BattlePresentationResult.PlayerDefeated:
                uiService?.ShowBattleResult(result);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    // Preserves existing UnityEvent and animation-event compatibility.
    public void ShowRus(bool boss1)
    {
        Show(boss1 ? BattlePresentationResult.Boss1Defeated : BattlePresentationResult.FinalVictory);
    }

    IEnumerator LoadBoss2AfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        pendingTransition = null;
        StartFade(false);
        sceneFlow?.TryLoad(ColorTimingSceneId.Boss2);
    }

    void StartFade(bool white)
    {
        fade = 0f;
        fadeColor = white ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (pendingTransition != null)
        {
            StopCoroutine(pendingTransition);
            pendingTransition = null;
        }
    }
}
