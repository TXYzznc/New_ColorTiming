using System;
using System.Collections.Generic;
using ColorTiming.Application.Battle;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Settings;
using UnityEngine;
using UnityEngine.UI;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Presentation.UI.Forms
{
    /// <summary>GF.UI-owned first-use weapon tutorial for one active battle.</summary>
    public sealed class BattleTutorialForm : UIFormBase, IColorTimingBattleTutorialForm
    {
        [SerializeField] private GameObject tipContent;
        [SerializeField] private Image weaponTipImage;
        [SerializeField] private Sprite[] weaponTips;

        private readonly HashSet<CombatWeaponType> shownWeaponTypes = new HashSet<CombatWeaponType>();
        private BattleSession session;
        private WeaponIdentity lastWeapon;
        private IGameInput gameInput;
        private IGameTime gameTime;
        private IColorTimingSettings settings;
        private IDisposable pauseLease;
        private float earliestDismissTime;

        public void BindRuntime(BattleSession runtimeSession, IGameInput input, IGameTime time, IColorTimingSettings projectSettings)
        {
            if (session != null)
            {
                session.SnapshotChanged -= OnSnapshotChanged;
            }

            session = runtimeSession ?? throw new ArgumentNullException(nameof(runtimeSession));
            gameInput = input ?? throw new ArgumentNullException(nameof(input));
            gameTime = time ?? throw new ArgumentNullException(nameof(time));
            settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            lastWeapon = session.Snapshot.Weapon;
            session.SnapshotChanged += OnSnapshotChanged;
            if (tipContent != null) tipContent.SetActive(false);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (session != null) session.SnapshotChanged -= OnSnapshotChanged;
            session = null;
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

        private void OnSnapshotChanged(BattleSnapshot snapshot)
        {
            if (snapshot.Weapon.Equals(lastWeapon)) return;
            lastWeapon = snapshot.Weapon;
            var weaponType = snapshot.Weapon.Type;
            if (settings == null || settings.KeyTipsDisabled || weaponType == CombatWeaponType.Normal
                || !shownWeaponTypes.Add(weaponType))
            {
                return;
            }

            var index = (int)weaponType - 1;
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
