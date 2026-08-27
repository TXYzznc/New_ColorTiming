using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ColorTiming.Presentation.UI
{
    /// <summary>GF.UI-owned first-use weapon tutorial for one active battle.</summary>
    public sealed class BattleTutorialForm : UIFormBase, IColorTimingBattleTutorialForm
    {
        [SerializeField] private GameObject tipContent;
        [SerializeField] private Image weaponTipImage;
        [SerializeField] private Sprite[] weaponTips;

        private readonly HashSet<WeaponType> shownWeaponTypes = new HashSet<WeaponType>();
        private HeroController hero;
        private IGameInput gameInput;
        private IGameTime gameTime;
        private IColorTimingSettings settings;
        private IDisposable pauseLease;
        private float earliestDismissTime;

        public void BindRuntime(HeroController runtimeHero, IGameInput input, IGameTime time, IColorTimingSettings projectSettings)
        {
            if (hero != null)
            {
                hero.OnSwitchWeapon.RemoveListener(OnSwitchWeapon);
            }

            hero = runtimeHero ?? throw new ArgumentNullException(nameof(runtimeHero));
            gameInput = input ?? throw new ArgumentNullException(nameof(input));
            gameTime = time ?? throw new ArgumentNullException(nameof(time));
            settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            hero.OnSwitchWeapon.AddListener(OnSwitchWeapon);
            if (tipContent != null) tipContent.SetActive(false);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (hero != null) hero.OnSwitchWeapon.RemoveListener(OnSwitchWeapon);
            hero = null;
            shownWeaponTypes.Clear();
            ReleasePause();
            base.OnClose(isShutdown, userData);
        }

        private void Update()
        {
            if (gameInput == null || tipContent == null || !tipContent.activeSelf || Time.unscaledTime < earliestDismissTime)
            {
                return;
            }

            if (gameInput.ConsumeAnyPressForOverlay())
            {
                tipContent.SetActive(false);
                ReleasePause();
            }
        }

        private void OnSwitchWeapon(Weapon weapon)
        {
            if (weapon == null || settings == null || settings.KeyTipsDisabled || weapon.weaponType == WeaponType.nor
                || !shownWeaponTypes.Add(weapon.weaponType))
            {
                return;
            }

            var index = (int)weapon.weaponType - 1;
            if (weaponTipImage == null || weaponTips == null || index < 0 || index >= weaponTips.Length)
            {
                throw new InvalidOperationException("BattleTutorial weapon-tip sprites are incomplete.");
            }

            weaponTipImage.sprite = weaponTips[index];
            tipContent.SetActive(true);
            earliestDismissTime = Time.unscaledTime + 2f;
            ReleasePause();
            pauseLease = gameTime.Acquire(0f);
        }

        private void ReleasePause()
        {
            pauseLease?.Dispose();
            pauseLease = null;
        }
    }
}
