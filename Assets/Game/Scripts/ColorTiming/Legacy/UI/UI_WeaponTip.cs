using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Input;
using ColorTiming.Combat;
using ColorTiming.Settings;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponTip : MonoBehaviour, IGameInputConsumer, IGameTimeConsumer, IColorTimingSettingsConsumer
{
    public GameObject showTip;
    public HeroController controller;
    public Image weaponTipImage;

    public Sprite[] weaponTips;

    List<WeaponType> cWeaponTips = new List<WeaponType>();

    float earliestDismissTime;
    IGameInput gameInput;
    IGameTime gameTime;
    IColorTimingSettings settings;
    IDisposable tutorialPause;

    public void BindGameInput(IGameInput input)
    {
        gameInput = input ?? throw new ArgumentNullException(nameof(input));
    }

    public void BindGameTime(IGameTime time)
    {
        gameTime = time ?? throw new ArgumentNullException(nameof(time));
    }

    public void BindSettings(IColorTimingSettings projectSettings)
    {
        settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
    }

    void Start()
    {
        controller?.OnSwitchWeapon.AddListener(OnSwitchWeapon);
    }

    private void OnSwitchWeapon(Weapon arg0)
    {
        if (settings != null && settings.KeyTipsDisabled) return;

        if (arg0.weaponType != WeaponType.nor)
        {
            if (!cWeaponTips.Contains(arg0.weaponType))
            {
                weaponTipImage.sprite = weaponTips[(int)arg0.weaponType - 1];
                cWeaponTips.Add(arg0.weaponType);

                
                showTip.SetActive(true);
                earliestDismissTime = Time.unscaledTime + 2f;
                tutorialPause?.Dispose();
                tutorialPause = gameTime?.Acquire(0f);

            }
        }
    }

    private void Update()
    {
        if (gameInput != null && showTip.activeSelf && Time.unscaledTime >= earliestDismissTime)
        {
            if (gameInput.ConsumeAnyPressForOverlay())
            {
                showTip.SetActive(false);
                ReleasePause();
            }
        }
    }

    void ReleasePause()
    {
        tutorialPause?.Dispose();
        tutorialPause = null;
    }

    private void OnDestroy()
    {
        controller?.OnSwitchWeapon.RemoveListener(OnSwitchWeapon);
        ReleasePause();
    }
}
