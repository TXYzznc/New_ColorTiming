// 文件职责：实现 战斗Hud GF.UI 表单及其交互生命周期。
// 所属模块：ColorTiming / Presentation / UI / Forms。

using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Configuration;
using ColorTiming.Presentation.UI.Components;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Presentation.UI.Presenters;
using UnityEngine;

namespace ColorTiming.Presentation.UI.Forms
{
    /// <summary>GF.UI-owned presentation for the active battle actors.</summary>
    public sealed class BattleHudForm : UIFormBase, IColorTimingBattleHudForm
    {
        [SerializeField] private BattlePlayerInfoView heroInfo;
        [SerializeField] private PlayerHealthPipsView heroHealth;
        [SerializeField] private BossHealthView bossHealth;

        // 绑定运行时依赖或事件监听。
        public void BindRuntime(IGameInput gameInput, IColorTimingUiService uiService,
            IColorTimingConfiguration configuration, BattleHudPresentation presentation)
        {
            if (gameInput == null) throw new ArgumentNullException(nameof(gameInput));
            if (uiService == null) throw new ArgumentNullException(nameof(uiService));
            if (presentation == null) throw new ArgumentNullException(nameof(presentation));
            if (heroInfo == null || heroHealth == null || bossHealth == null)
            {
                throw new InvalidOperationException("BattleHudForm serialized references are incomplete.");
            }

            heroInfo.BindGameInput(gameInput);
            heroInfo.BindUiService(uiService);
            heroInfo.BindConfiguration(configuration);
            heroHealth.Configure(configuration.Presentation.PlayerPipSpacing,
                configuration.Presentation.PlayerPipAlternateOffset);
            var sceneId = presentation.Session.Kind == BattleKind.Boss1
                ? ColorTimingSceneId.Boss1
                : ColorTimingSceneId.Boss2;
            var battle = configuration.GetBattle(sceneId);
            var boss = configuration.GetBoss(battle.BossId);
            bossHealth.Configure(configuration.Presentation.BossPipFloatSpeed,
                configuration.Presentation.BossPipMinY, configuration.Presentation.BossPipMaxY,
                boss.UpcomingLimit);
            heroInfo.BindSession(presentation.Session);
            heroHealth.Bind(presentation.Session);

            bossHealth.Bind(presentation.Session);
        }

        // 在 GF UI 表单关闭时停止流程并清理临时状态。
        protected override void OnClose(bool isShutdown, object userData)
        {
            if (heroInfo != null) heroInfo.BindSession(null);
            if (heroHealth != null) heroHealth.Bind(null);
            if (bossHealth != null) bossHealth.Bind(null);
            base.OnClose(isShutdown, userData);
        }
    }
}
