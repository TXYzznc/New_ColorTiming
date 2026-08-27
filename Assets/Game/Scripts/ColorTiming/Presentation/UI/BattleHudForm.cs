using System;
using ColorTiming.Input;
using UnityEngine;

namespace ColorTiming.Presentation.UI
{
    /// <summary>GF.UI-owned presentation for the active battle actors.</summary>
    public sealed class BattleHudForm : UIFormBase, IColorTimingBattleHudForm
    {
        [SerializeField] private UI_HeroInfo heroInfo;
        [SerializeField] private UI_HeroHPBox heroHealth;
        [SerializeField] private UI_BossHPController boss1Health;
        [SerializeField] private UI_BossHPController2 boss2Health;

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
            heroInfo.BindHero(presentation.Hero);
            heroHealth.Bind(presentation.Hero);

            var isBoss1 = presentation.Boss1 != null;
            boss1Health.enabled = isBoss1;
            boss2Health.enabled = !isBoss1;
            boss1Health.Bind(presentation.Boss1);
            boss2Health.Bind(presentation.Boss2);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (heroInfo != null) heroInfo.BindHero(null);
            if (heroHealth != null) heroHealth.Bind(null);
            if (boss1Health != null) boss1Health.Bind(null);
            if (boss2Health != null) boss2Health.Bind(null);
            base.OnClose(isShutdown, userData);
        }
    }
}
