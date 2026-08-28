using System;
using System.Collections.Generic;
using ColorTiming.Combat;

namespace ColorTiming.Player
{
    /// <summary>
    /// Deterministic decision boundary shared by both boss-stage weapon spawners.
    /// Scene placement and prefab lifetime remain in the Unity-facing presenter.
    /// </summary>
    public sealed class WeaponSpawnerRuntime
    {
        private readonly WeaponSpawnClock clock;
        private readonly WeaponSpawnPolicy policy;
        private readonly IRandomSource random;

        public WeaponSpawnerRuntime(float interval, WeaponSpawnPolicy policy, IRandomSource random)
        {
            clock = new WeaponSpawnClock(interval);
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public WeaponSpawnDecision Tick(
            float deltaTime,
            IReadOnlyCollection<WeaponColor> activeColors,
            WeaponColor currentWeakness)
        {
            return clock.Tick(deltaTime)
                ? policy.Decide(activeColors, currentWeakness, random)
                : default;
        }
    }
}
