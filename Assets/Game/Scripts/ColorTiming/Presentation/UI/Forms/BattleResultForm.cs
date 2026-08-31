// 文件职责：实现 战斗结果 GF.UI 表单及其交互生命周期。
// 所属模块：ColorTiming / Presentation / UI / Forms。

using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Input;
using ColorTiming.Configuration;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Presentation.UI.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI.Forms
{
/// <summary>Shared GF.UI presentation for final victory and defeat results.</summary>
public sealed class BattleResultForm : UIFormBase, IColorTimingBattleResultForm,
    IColorTimingPresentationConfigurationConsumer
{
    [SerializeField] Image victory;
    [SerializeField] GameObject defeat;

    IColorTimingSceneFlow sceneFlow;
    IGameInput gameInput;
    float fade;
    float fadeSpeed;

    public void BindPresentationConfiguration(ColorTimingPresentationTable configuration) =>
        fadeSpeed = configuration.ResultFadeSpeed;

    // 绑定运行时依赖或事件监听。
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

    // 逐帧推进需要实时刷新的业务或表现状态。
    void Update()
    {
        if (victory != null && victory.gameObject.activeSelf && fade < 1f)
        {
            fade = Mathf.Min(1f, fade + Time.unscaledDeltaTime * fadeSpeed);
            victory.color = new Color(1f, 1f, 1f, fade);
        }

        if (gameInput != null && gameInput.ConfirmPressed)
        {
            gameInput.ConsumeAnyPressForOverlay();
            sceneFlow?.TryLoad(ColorTimingSceneId.StartMenu);
        }
    }
}
}
