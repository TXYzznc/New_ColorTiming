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

    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    public void BindSettings(IColorTimingSettings projectSettings)
    {
        settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
        SetOffTip(settings.KeyTipsDisabled);
    }

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


    private void Start()
    {

        if (settings != null) SetOffTip(settings.KeyTipsDisabled);
    }

    public void BackMenu()
    {
        sceneFlow?.TryLoad(ColorTimingSceneId.StartMenu);

    }

    public void OffKeyTip()
    {
        if (settings != null) settings.KeyTipsDisabled = true;
        SetOffTip(true);

    }

    public void OpenKeyTip()
    {
        if (settings != null) settings.KeyTipsDisabled = false;
        SetOffTip(false);
    }

    void SetOffTip(bool off)
    {
        offTipButton.SetActive(!off);
        openTipButton.SetActive(off);
    }

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
