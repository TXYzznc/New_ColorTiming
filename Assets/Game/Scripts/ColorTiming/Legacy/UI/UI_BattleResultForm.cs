using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Input;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared GF.UI presentation for final victory and defeat results.</summary>
public sealed class UI_BattleResultForm : UIFormBase, IColorTimingBattleResultForm
{
    [SerializeField] Image victory;
    [SerializeField] GameObject defeat;

    IColorTimingSceneFlow sceneFlow;
    IGameInput gameInput;
    float fade;

    public void BindRuntime(
        IColorTimingSceneFlow flow,
        IGameInput input,
        BattlePresentationResult result)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
        bool showVictory = result != BattlePresentationResult.PlayerDefeated;
        if (victory != null)
        {
            victory.gameObject.SetActive(showVictory);
            victory.color = new Color(1f, 1f, 1f, 0f);
        }
        if (defeat != null)
        {
            defeat.SetActive(!showVictory);
        }
        fade = 0f;
    }

    void Update()
    {
        if (victory != null && victory.gameObject.activeSelf && fade < 1f)
        {
            fade = Mathf.Min(1f, fade + Time.unscaledDeltaTime);
            victory.color = new Color(1f, 1f, 1f, fade);
        }

        if (gameInput != null && gameInput.ConfirmPressed)
        {
            gameInput.ConsumeAnyPressForOverlay();
            sceneFlow?.TryLoad(ColorTimingSceneId.StartMenu);
        }
    }
}
