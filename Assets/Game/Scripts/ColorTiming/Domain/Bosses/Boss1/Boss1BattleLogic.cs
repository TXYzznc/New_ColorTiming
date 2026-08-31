// 文件职责：实现 Boss1战斗 的核心业务规则。
// 所属模块：ColorTiming / Domain / Bosses / Boss1。

using System;
using ColorTiming.Configuration;

namespace ColorTiming.Bosses.Boss1
{
    public enum Boss1DistanceZone
    {
        Near,
        Middle,
        Far,
    }

    public enum Boss1Attack
    {
        Attack1 = 1,
        Attack2 = 2,
        Attack3 = 3,
        Attack4 = 4,
        Attack5 = 5,
        Attack6 = 6,
    }

    public static class Boss1DistanceZones
    {
        // 执行Resolve对应的主要流程。
        public static Boss1DistanceZone Resolve(bool insideNear, bool insideMiddle)
        {
            if (insideNear)
            {
                return Boss1DistanceZone.Near;
            }
            return insideMiddle ? Boss1DistanceZone.Middle : Boss1DistanceZone.Far;
        }
    }

    public sealed class Boss1AttackSelector
    {
        private readonly Boss1AttackRules rules;

        public Boss1AttackSelector(Boss1AttackRules rules)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public Boss1Attack? LastAttack { get; private set; }

        // 执行Select对应的主要流程。
        public Boss1Attack Select(Boss1DistanceZone zone, float sample)
        {
            if (sample < 0f || sample >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(sample));
            }

            var candidates = rules.For(zone);
            var cumulative = 0f;
            var selected = candidates[candidates.Count - 1];
            for (var index = 0; index < candidates.Count; index++)
            {
                cumulative += candidates[index].Weight;
                if (sample < cumulative)
                {
                    selected = candidates[index];
                    break;
                }
            }

            var attack = selected.DisallowRepeat && LastAttack == selected.Attack
                ? selected.Fallback
                : selected.Attack;
            LastAttack = attack;
            return attack;
        }
    }

    public sealed class Boss1AttackCycle
    {
        // 初始化Boss1攻击Cycle实例及其核心依赖。
        public Boss1AttackCycle(float initialCooldown)
        {
            SetCooldown(initialCooldown);
        }

        public float CooldownRemaining { get; private set; }
        public bool IsAttacking { get; private set; }

        // 按当前时间步推进核心状态，并发布必要的状态变化。
        public bool Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            if (IsAttacking)
            {
                return false;
            }

            CooldownRemaining = Math.Max(0f, CooldownRemaining - deltaTime);
            return CooldownRemaining <= 0f;
        }

        // 执行开始攻击对应的主要流程。
        public bool BeginAttack()
        {
            if (IsAttacking || CooldownRemaining > 0f)
            {
                return false;
            }

            IsAttacking = true;
            return true;
        }

        // 执行完成攻击对应的主要流程。
        public void CompleteAttack(float nextCooldown)
        {
            IsAttacking = false;
            SetCooldown(nextCooldown);
        }

        // 设置冷却，并使后续流程使用最新状态。
        private void SetCooldown(float seconds)
        {
            if (seconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }
            CooldownRemaining = seconds;
        }
    }
}
