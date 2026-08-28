// 文件职责：定义 战斗契约，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

using System;

namespace ColorTiming.Combat
{
    public readonly struct CombatPoint
    {
        // 初始化CombatPoint实例及其核心依赖。
        public CombatPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    /// <summary>Stable runtime identity used across domain and presentation boundaries.</summary>
    public readonly struct ActorId : IEquatable<ActorId>
    {
        public static readonly ActorId Player = new ActorId(1);
        public static readonly ActorId BossHead = new ActorId(2);
        public static readonly ActorId BossTail = new ActorId(3);

        // 初始化ActorID实例及其核心依赖。
        public ActorId(int value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            Value = value;
        }

        public int Value { get; }
        public bool Equals(ActorId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ActorId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Actor:{Value}";
        public static bool operator ==(ActorId left, ActorId right) => left.Equals(right);
        public static bool operator !=(ActorId left, ActorId right) => !left.Equals(right);
    }

    public enum BattleKind
    {
        Boss1,
        Boss2,
    }

    public enum BattleLifecycle
    {
        Running,
        Paused,
        Victory,
        Defeat,
        Disposed,
    }

    /// <summary>Unity-independent damage data submitted by a hitbox adapter.</summary>
    public readonly struct BattleDamage
    {
        // 初始化战斗伤害实例及其核心依赖。
        public BattleDamage(
            ActorId attacker,
            ActorId target,
            WeaponIdentity weapon,
            CombatPoint contactPoint,
            string parameter = "")
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            ContactPoint = contactPoint;
            Parameter = parameter ?? string.Empty;
        }

        public ActorId Attacker { get; }
        public ActorId Target { get; }
        public WeaponIdentity Weapon { get; }
        public CombatPoint ContactPoint { get; }
        public string Parameter { get; }
        public bool IsInstantKill => Parameter.IndexOf("miaosha", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
