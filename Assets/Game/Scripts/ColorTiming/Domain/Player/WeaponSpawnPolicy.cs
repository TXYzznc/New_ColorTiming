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

        public WeaponSpawnClock(float interval)
        {
            if (interval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(interval));
            }

            this.interval = interval;
        }

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
        private readonly WeaponColor[] allowedColors;
        private readonly CombatWeaponType[] allowedTypes;
        private readonly int activeLimit;
        private readonly int guaranteeThreshold;

        public WeaponSpawnPolicy(
            IEnumerable<WeaponColor> allowedColors,
            IEnumerable<CombatWeaponType> allowedTypes,
            int activeLimit,
            int guaranteeThreshold = 3)
        {
            this.allowedColors = allowedColors?.Distinct().ToArray()
                ?? throw new ArgumentNullException(nameof(allowedColors));
            this.allowedTypes = allowedTypes?.Where(type => type != CombatWeaponType.Normal).Distinct().ToArray()
                ?? throw new ArgumentNullException(nameof(allowedTypes));
            if (this.allowedColors.Length == 0 || this.allowedTypes.Length == 0)
            {
                throw new ArgumentException("Weapon spawning needs at least one color and one non-normal type.");
            }
            if (activeLimit <= 0 || guaranteeThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(activeLimit));
            }

            this.activeLimit = activeLimit;
            this.guaranteeThreshold = guaranteeThreshold;
        }

        public static WeaponSpawnPolicy Boss1(int activeLimit = 5)
        {
            return new WeaponSpawnPolicy(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple },
                new[] { CombatWeaponType.Scissors, CombatWeaponType.Hammer, CombatWeaponType.Bomb },
                activeLimit);
        }

        public static WeaponSpawnPolicy Boss2(int activeLimit = 10)
        {
            return new WeaponSpawnPolicy(
                new[] { WeaponColor.Red, WeaponColor.Green, WeaponColor.Purple, WeaponColor.Orange },
                new[] { CombatWeaponType.Knife, CombatWeaponType.Axe, CombatWeaponType.Airplane },
                activeLimit);
        }

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

            var mustGuaranteeWeakness = activeColors.Count >= guaranteeThreshold
                && !activeColors.Contains(currentWeakness);
            var color = mustGuaranteeWeakness
                ? currentWeakness
                : allowedColors[random.Range(0, allowedColors.Length)];
            var type = allowedTypes[random.Range(0, allowedTypes.Length)];
            return new WeaponSpawnDecision(true, new WeaponIdentity(color, type));
        }
    }
}
