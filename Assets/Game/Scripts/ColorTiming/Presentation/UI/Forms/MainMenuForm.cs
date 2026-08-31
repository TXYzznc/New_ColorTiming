// 文件职责：实现 Main菜单 GF.UI 表单及其交互生命周期。
// 所属模块：ColorTiming / Presentation / UI / Forms。

using UnityEngine;
using UnityEngine.Audio;
using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Settings;
using ColorTiming.Presentation.Audio;
using GameFramework;
using UnityGameFramework.Runtime;

namespace ColorTiming.Presentation.UI.Forms
{
public class MainMenuForm : UIFormBase, IColorTimingSceneFlowConsumer, IColorTimingSettingsConsumer, IColorTimingSoundConsumer,
    IColorTimingStartMenuForm
{
    IColorTimingSceneFlow sceneFlow;
    IColorTimingSettings settings;
    IColorTimingSoundService soundService;
    MainMenuIntroSequence videoSequence;
    UiSoundView uiSound;
    int bgmSoundId;
    public IUiSoundSink UiSound => uiSound;

    // 绑定场景流程依赖或事件监听。
    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    // 绑定设置依赖或事件监听。
    public void BindSettings(IColorTimingSettings projectSettings)
    {
        settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
        RefreshSettingsView();
    }

    // 绑定运行时依赖或事件监听。
    public void BindRuntime(
        IColorTimingSceneFlow flow,
        IColorTimingSettings projectSettings,
        IColorTimingSoundService projectSoundService)
    {
        BindSceneFlow(flow);
        BindSettings(projectSettings);
        BindSoundService(projectSoundService);
        uiSound = GetComponentInChildren<UiSoundView>(true);
        if (uiSound == null) throw new InvalidOperationException("MainMenu requires one UiSoundView.");
        uiSound.BindSoundService(projectSoundService);
        foreach (var consumer in GetComponentsInChildren<UiButtonSoundView>(true)) consumer.BindUiSound(uiSound);
    }

    // 绑定音效Service依赖或事件监听。
    public void BindSoundService(IColorTimingSoundService projectSoundService)
    {
        soundService = projectSoundService ?? throw new ArgumentNullException(nameof(projectSoundService));
        RefreshBgmPlayback();
    }

    public GameObject StartBtnBox;
    public GameObject GoButtonBox;
    public GameObject SettingButtonBox;

    public AudioMixer AudioMixer;
    [SerializeField] AudioClip bgm;

    public GameObject BGMBtn_Open;
    public GameObject BGMBtn_Off;

    public GameObject SFXBtn_Open;
    public GameObject SFXBtn_Off;

    public GameObject offTipButton;
    public GameObject openTipButton;

    // 在 GF UI 表单打开时接收参数并刷新显示。
    protected override void OnOpen(object userData)
    {
        Log.Info(
            "[ColorTiming.UIFlow] action=MainMenuForm.OnOpen id={0} frame={1} realtime={2:0.000}",
            Id,
            Time.frameCount,
            Time.realtimeSinceStartup);
        // GF.UI pools forms, so authored navigation state must be restored on every open.
        if (StartBtnBox != null) StartBtnBox.SetActive(true);
        if (GoButtonBox != null) GoButtonBox.SetActive(false);
        if (SettingButtonBox != null) SettingButtonBox.SetActive(false);
        base.OnOpen(userData);
        videoSequence ??= GetComponentInChildren<MainMenuIntroSequence>(true);
        videoSequence?.RestartSequence();
        RefreshBgmPlayback();
    }

    // 在 GF UI 表单关闭时停止流程并清理临时状态。
    protected override void OnClose(bool isShutdown, object userData)
    {
        Log.Info(
            "[ColorTiming.UIFlow] action=MainMenuForm.OnClose id={0} isShutdown={1} frame={2} realtime={3:0.000}",
            Id,
            isShutdown,
            Time.frameCount,
            Time.realtimeSinceStartup);
        videoSequence?.StopSequence();
        StopBgmPlayback();
        base.OnClose(isShutdown, userData);
    }

    // 执行StartGameBtnDown对应的主要流程。
    public void StartGameBtnDown()
    {

        StartBtnBox.SetActive(false);
        GoButtonBox.SetActive(true);
    }

    // 执行BackStartBtnDown对应的主要流程。
    public void BackStartBtnDown()
    {
        StartBtnBox.SetActive(true);
        GoButtonBox.SetActive(false);
    }

    // 设置tingBtnDwon，并使后续流程使用最新状态。
    public void SettingBtnDwon()
    {
        SettingButtonBox.SetActive(true);
        StartBtnBox.SetActive(false);
    }

    // 执行Back设置BtnDwon对应的主要流程。
    public void BackSettingBtnDwon()
    {
        StartBtnBox.SetActive(true);
        SettingButtonBox.SetActive(false);
    }

    // 执行GoTest1对应的主要流程。
    public void GoTest1()
    {
        bool accepted = sceneFlow?.TryLoad(ColorTimingSceneId.Boss1) ?? false;
        Log.Info(
            "[ColorTiming.SceneFlow] action=MainMenu.Request target=Boss1 result={0} frame={1} realtime={2:0.000}",
            accepted ? "Accepted" : "Rejected",
            Time.frameCount,
            Time.realtimeSinceStartup);
    }


    // 执行GoTest2对应的主要流程。
    public void GoTest2()
    {
        bool accepted = sceneFlow?.TryLoad(ColorTimingSceneId.Boss2) ?? false;
        Log.Info(
            "[ColorTiming.SceneFlow] action=MainMenu.Request target=Boss2 result={0} frame={1} realtime={2:0.000}",
            accepted ? "Accepted" : "Rejected",
            Time.frameCount,
            Time.realtimeSinceStartup);
    }

    // 执行ExitGameBtn对应的主要流程。
    public void ExitGameBtn()
    {
        GameEntry.Shutdown(ShutdownType.Quit);
    }


    // 显示SystemSetUP并同步当前数据。
    public void ShowSystemSetUP()
    {

    }

    // 设置BGM，并使后续流程使用最新状态。
    public void SetBGM(bool open)
    {
        if (settings != null)
        {
            settings.BgmEnabled = open;
        }
        SetToggleView(BGMBtn_Open, BGMBtn_Off, open);
        RefreshBgmPlayback();
    }

    // 设置SFX，并使后续流程使用最新状态。
    public void SetSFX(bool open)
    {
        if (settings != null)
        {
            settings.SfxEnabled = open;
        }
        SetToggleView(SFXBtn_Open, SFXBtn_Off, open);
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

    // 根据最新数据刷新设置视图。
    void RefreshSettingsView()
    {
        if (settings == null) return;
        SetToggleView(BGMBtn_Open, BGMBtn_Off, settings.BgmEnabled);
        SetToggleView(SFXBtn_Open, SFXBtn_Off, settings.SfxEnabled);
        SetOffTip(settings.KeyTipsDisabled);
        RefreshBgmPlayback();
    }

    // 根据最新数据刷新BgmPlayback。
    void RefreshBgmPlayback()
    {
        if (!isActiveAndEnabled || soundService == null || settings == null || !settings.BgmEnabled || bgm == null)
        {
            StopBgmPlayback();
            return;
        }
        if (bgmSoundId <= 0)
        {
            bgmSoundId = soundService.Play(bgm, ColorTimingSoundChannel.BGM, transform.position, true);
        }
    }

    // 停止BgmPlayback并清理临时播放状态。
    void StopBgmPlayback()
    {
        if (bgmSoundId <= 0)
        {
            return;
        }
        soundService?.Stop(bgmSoundId);
        bgmSoundId = 0;
    }

    // 设置Toggle视图，并使后续流程使用最新状态。
    static void SetToggleView(GameObject openButton, GameObject offButton, bool enabled)
    {
        if (openButton != null) openButton.SetActive(!enabled);
        if (offButton != null) offButton.SetActive(enabled);
    }
}
}
