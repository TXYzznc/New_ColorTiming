// 文件职责：定义 武器生成Policy，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Player。

using System;
using System.Collections.Generic;
using System.Linq;
using ColorTiming.Combat;
using CombatWeaponType = ColorTiming.Combat.WeaponType;

namespace ColorTiming.Player
{
    public sealed class WeaponSpawnClock
    {
        private readonly float interval;
        private float remaining;

        // 初始化武器生成Clock实例及其核心依赖。
        public WeaponSpawnClock(float interval)
        {
            if (interval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            this.interval = interval;
        }

        // 按当前时间步推进核心状态，并发布必要的状态变化。
        public bool Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (remaining > 0f)
            {
                remaining -= deltaTime;
                if (remaining > 0f)
                {
                    return false;
                }

                // Preserve overshoot so variable frame times do not lengthen the cadence.
                do
                {
                    remaining += interval;
                }
                while (remaining <= 0f);
                return true;
            }

            remaining = interval;
            return true;
        }
    }

    public readonly struct WeaponSpawnDecision
    {
        // 初始化武器生成Decision实例及其核心依赖。
        public WeaponSpawnDecision(bool shouldSpawn, WeaponIdentity weapon)
        {
            ShouldSpawn = shouldSpawn;
            Weapon = weapon;
        }

        public bool ShouldSpawn { get; }
        public WeaponIdentity Weapon { get; }
    }

    public sealed class WeaponSpawnPolicy
    {
        private readonly WeaponIdentity[] allowedWeapons;
        private readonly int activeLimit;
        private readonly int guaranteeThreshold;

        // 初始化武器生成Policy实例及其核心依赖。
        public WeaponSpawnPolicy(
            IEnumerable<WeaponIdentity> allowedWeapons,
            int activeLimit,
            int guaranteeThreshold = 3)
        {
            this.allowedWeapons = allowedWeapons?.Where(weapon => !weapon.IsNormal).Distinct().ToArray()
                ?? throw new ArgumentNullException(nameof(allowedWeapons));
            if (this.allowedWeapons.Length == 0)
            {
                throw new ArgumentException("Weapon spawning needs at least one supported non-normal weapon.");
            }
            if (activeLimit <= 0 || guaranteeThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(activeLimit));
            }

            this.activeLimit = activeLimit;
            this.guaranteeThreshold = guaranteeThreshold;
        }

        // 执行Decide对应的主要流程。
        public WeaponSpawnDecision Decide(
            IReadOnlyCollection<WeaponColor> activeColors,
            WeaponColor currentWeakness,
            IRandomSource random)
        {
            if (activeColors == null)
            {
                throw new ArgumentNullException(nameof(activeColors));
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            if (activeColors.Count >= activeLimit)
            {
                return default;
            }

            var matchingWeakness = allowedWeapons.Where(weapon => weapon.Color == currentWeakness).ToArray();
            var mustGuaranteeWeakness = activeColors.Count >= guaranteeThreshold
                && !activeColors.Contains(currentWeakness)
                && matchingWeakness.Length > 0;
            var candidates = mustGuaranteeWeakness ? matchingWeakness : allowedWeapons;
            return new WeaponSpawnDecision(true, candidates[random.Range(0, candidates.Length)]);
        }
    }
}
