using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Settings;
using ColorTiming.Presentation.UI;

public class UI_ESC : UIFormBase, IColorTimingSceneFlowConsumer, IColorTimingSettingsConsumer,
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

    public void BindRuntime(IColorTimingSceneFlow flow, IColorTimingSettings projectSettings)
    {
        BindSceneFlow(flow);
        BindSettings(projectSettings);
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
