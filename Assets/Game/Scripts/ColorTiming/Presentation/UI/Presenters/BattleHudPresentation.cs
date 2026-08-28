// 文件职责：定义 战斗Hud展示，承担 Presenters 模块中的对应职责。
// 所属模块：ColorTiming / Presentation / UI / Presenters。

using System;
using ColorTiming.Application.Battle;

namespace ColorTiming.Presentation.UI.Presenters
{
    /// <summary>Session-backed presentation source required by the battle HUD.</summary>
    public sealed class BattleHudPresentation
    {
        // 初始化战斗Hud展示实例及其核心依赖。
        public BattleHudPresentation(BattleSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public BattleSession Session { get; }
    }
}
