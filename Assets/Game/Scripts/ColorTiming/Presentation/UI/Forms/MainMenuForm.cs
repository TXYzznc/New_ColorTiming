
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

    public void BindSceneFlow(IColorTimingSceneFlow flow)
    {
        sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
    }

    public void BindSettings(IColorTimingSettings projectSettings)
    {
        settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
        RefreshSettingsView();
    }

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

    protected override void OnOpen(object userData)
    {
        // GF.UI pools forms, so authored navigation state must be restored on every open.
        if (StartBtnBox != null) StartBtnBox.SetActive(true);
        if (GoButtonBox != null) GoButtonBox.SetActive(false);
        if (SettingButtonBox != null) SettingButtonBox.SetActive(false);
        base.OnOpen(userData);
        videoSequence ??= GetComponentInChildren<MainMenuIntroSequence>(true);
        videoSequence?.RestartSequence();
        RefreshBgmPlayback();
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        videoSequence?.StopSequence();
        StopBgmPlayback();
        base.OnClose(isShutdown, userData);
    }

    public void StartGameBtnDown()
    {

        StartBtnBox.SetActive(false);
        GoButtonBox.SetActive(true);
    }

    public void BackStartBtnDown()
    {
        StartBtnBox.SetActive(true);
        GoButtonBox.SetActive(false);
    }

    public void SettingBtnDwon()
    {
        SettingButtonBox.SetActive(true);
        StartBtnBox.SetActive(false);
    }

    public void BackSettingBtnDwon()
    {
        StartBtnBox.SetActive(true);
        SettingButtonBox.SetActive(false);
    }

    public void GoTest1()
    {
        sceneFlow?.TryLoad(ColorTimingSceneId.Boss1);
    }


    public void GoTest2()
    {
        sceneFlow?.TryLoad(ColorTimingSceneId.Boss2);
    }

    public void ExitGameBtn()
    {
        GameEntry.Shutdown(ShutdownType.Quit);
    }


    public void ShowSystemSetUP()
    {

    }

    public void SetBGM(bool open)
    {
        if (settings != null)
        {
            settings.BgmEnabled = open;
        }
        SetToggleView(BGMBtn_Open, BGMBtn_Off, open);
        RefreshBgmPlayback();
    }

    public void SetSFX(bool open)
    {
        if (settings != null)
        {
            settings.SfxEnabled = open;
        }
        SetToggleView(SFXBtn_Open, SFXBtn_Off, open);
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

    void RefreshSettingsView()
    {
        if (settings == null) return;
        SetToggleView(BGMBtn_Open, BGMBtn_Off, settings.BgmEnabled);
        SetToggleView(SFXBtn_Open, SFXBtn_Off, settings.SfxEnabled);
        SetOffTip(settings.KeyTipsDisabled);
        RefreshBgmPlayback();
    }

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

    void StopBgmPlayback()
    {
        if (bgmSoundId <= 0)
        {
            return;
        }
        soundService?.Stop(bgmSoundId);
        bgmSoundId = 0;
    }

    static void SetToggleView(GameObject openButton, GameObject offButton, bool enabled)
    {
        if (openButton != null) openButton.SetActive(!enabled);
        if (offButton != null) offButton.SetActive(enabled);
    }
}
}
