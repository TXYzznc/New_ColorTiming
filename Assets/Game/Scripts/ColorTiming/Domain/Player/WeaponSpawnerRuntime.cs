// 文件职责：定义 武器Spawner运行时，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Player。

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

        // 初始化武器Spawner运行时实例及其核心依赖。
        public WeaponSpawnerRuntime(float interval, WeaponSpawnPolicy policy, IRandomSource random)
        {
            clock = new WeaponSpawnClock(interval);
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // 按当前时间步推进核心状态，并发布必要的状态变化。
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
