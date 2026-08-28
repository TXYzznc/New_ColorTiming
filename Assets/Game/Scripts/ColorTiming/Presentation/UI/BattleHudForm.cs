using System;
using ColorTiming.Input;
using UnityEngine;

namespace ColorTiming.Presentation.UI
{
    /// <summary>GF.UI-owned presentation for the active battle actors.</summary>
    public sealed class BattleHudForm : UIFormBase, IColorTimingBattleHudForm
    {
        [SerializeField] private BattlePlayerInfoView heroInfo;
        [SerializeField] private PlayerHealthPipsView heroHealth;
        [SerializeField] private Boss1HealthView boss1Health;
        [SerializeField] private Boss2HealthView boss2Health;

        public void BindRuntime(IGameInput gameInput, IColorTimingUiService uiService, BattleHudPresentation presentation)
        {
            if (gameInput == null) throw new ArgumentNullException(nameof(gameInput));
            if (uiService == null) throw new ArgumentNullException(nameof(uiService));
            if (presentation == null) throw new ArgumentNullException(nameof(presentation));
            if (heroInfo == null || heroHealth == null || boss1Health == null || boss2Health == null)
            {
                throw new InvalidOperationException("BattleHudForm serialized references are incomplete.");
            }

            heroInfo.BindGameInput(gameInput);
            heroInfo.BindUiService(uiService);
            heroInfo.BindSession(presentation.Session);
            heroHealth.Bind(presentation.Session);

            var isBoss1 = presentation.Session.Kind == ColorTiming.Combat.BattleKind.Boss1;
            boss1Health.enabled = isBoss1;
            boss2Health.enabled = !isBoss1;
            boss1Health.Bind(isBoss1 ? presentation.Session : null);
            boss2Health.Bind(isBoss1 ? null : presentation.Session);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (heroInfo != null) heroInfo.BindSession(null);
            if (heroHealth != null) heroHealth.Bind(null);
            if (boss1Health != null) boss1Health.Bind(null);
            if (boss2Health != null) boss2Health.Bind(null);
            base.OnClose(isShutdown, userData);
        }
    }
}
