// 文件职责：实现 暂停菜单 GF.UI 表单及其交互生命周期。
// 所属模块：ColorTiming / Presentation / UI / Forms。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Settings;
using ColorTiming.Presentation.Audio;

namespace ColorTiming.Presentation.UI.Forms
{
public class PauseMenuForm : UIFormBase, IColorTimingSceneFlowConsumer, IColorTimingSettingsConsumer,
    IColorTimingPauseForm
{
    IColorTimingSceneFlow sceneFlow;
    IColorTimingSettings settings;

    // 绑定场景流程依赖或事件监听。
    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    // 绑定设置依赖或事件监听。
    public void BindSettings(IColorTimingSettings projectSettings)
    {
        settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
        SetOffTip(settings.KeyTipsDisabled);
    }

    // 绑定运行时依赖或事件监听。
    public void BindRuntime(IColorTimingSceneFlow flow, IColorTimingSettings projectSettings, IUiSoundSink uiSound)
    {
        BindSceneFlow(flow);
        BindSettings(projectSettings);
        if (uiSound == null) throw new ArgumentNullException(nameof(uiSound));
        foreach (var consumer in GetComponentsInChildren<UiButtonSoundView>(true)) consumer.BindUiSound(uiSound);
    }
    public GameObject offTipButton;
    public GameObject openTipButton;

    //public GameObject nextBtn;
    //public GameObject lastBtn;


    // 在首帧启动依赖就绪后的业务或表现流程。
    private void Start()
    {

        if (settings != null) SetOffTip(settings.KeyTipsDisabled);
    }

    // 执行Back菜单对应的主要流程。
    public void BackMenu()
    {
        sceneFlow?.TryLoad(ColorTimingSceneId.StartMenu);

    }

    // 执行OffKeyTip对应的主要流程。
    public void OffKeyTip()
    {
        if (settings != null) settings.KeyTipsDisabled = true;
        SetOffTip(true);

    }

    // 打开KeyTip并传入本次使用参数。
    public void OpenKeyTip()
    {
        if (settings != null) settings.KeyTipsDisabled = false;
        SetOffTip(false);
    }

    // 设置OffTip，并使后续流程使用最新状态。
    void SetOffTip(bool off)
    {
        offTipButton.SetActive(!off);
        openTipButton.SetActive(off);
    }

    // 执行GoNextLevel对应的主要流程。
    public void GoNextLevel(int l)
    {
        if (l > 1)
        {
            sceneFlow?.TryLoad(ColorTimingSceneId.Boss2, true);
        }
        else
        {
            sceneFlow?.TryLoad(ColorTimingSceneId.Boss1, true);
        }

    }
}
}
