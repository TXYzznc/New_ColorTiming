// 文件职责：定义 战斗快照 数据及其状态语义。
// 所属模块：ColorTiming / Application / Battle。

using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Player;

namespace ColorTiming.Application.Battle
{
    /// <summary>Immutable presentation state; created only when authoritative state changes.</summary>
    public sealed class BattleSnapshot
    {
        public BattleSnapshot(
            long version,
            BattleKind kind,
            BattleLifecycle lifecycle,
            int playerHealth,
            int playerMaximumHealth,
            PlayerActionState playerAction,
            WeaponIdentity weapon,
            IReadOnlyList<WeaponColor> weaknesses,
            bool bossDamageable,
            bool bossTailActive)
        {
            Version = version;
            Kind = kind;
            Lifecycle = lifecycle;
            PlayerHealth = playerHealth;
            PlayerMaximumHealth = playerMaximumHealth;
            PlayerAction = playerAction;
            Weapon = weapon;
            Weaknesses = weaknesses ?? throw new ArgumentNullException(nameof(weaknesses));
            BossDamageable = bossDamageable;
            BossTailActive = bossTailActive;
        }

        public long Version { get; }
        public BattleKind Kind { get; }
        public BattleLifecycle Lifecycle { get; }
        public int PlayerHealth { get; }
        public int PlayerMaximumHealth { get; }
        public PlayerActionState PlayerAction { get; }
        public WeaponIdentity Weapon { get; }
        public IReadOnlyList<WeaponColor> Weaknesses { get; }
        public bool BossDamageable { get; }
        public bool BossTailActive { get; }
    }

    public enum BattlePresentationEventKind
    {
        PlayerDamaged,
        PlayerHealed,
        PlayerWeaponChanged,
        BossDamaged,
        BossTailActivated,
        BattleWon,
        BattleLost,
    }

    public readonly struct BattlePresentationEvent
    {
        // 初始化战斗展示事件实例及其核心依赖。
        public BattlePresentationEvent(BattlePresentationEventKind kind, WeaponColor color = WeaponColor.Red)
        {
            Kind = kind;
            Color = color;
        }

        public BattlePresentationEventKind Kind { get; }
        public WeaponColor Color { get; }
    }
}
