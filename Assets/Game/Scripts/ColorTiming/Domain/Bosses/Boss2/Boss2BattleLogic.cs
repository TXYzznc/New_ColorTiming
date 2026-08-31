// 文件职责：实现 Boss2战斗 的核心业务规则。
// 所属模块：ColorTiming / Domain / Bosses / Boss2。

using System;
using ColorTiming.Configuration;

namespace ColorTiming.Bosses.Boss2
{
    public enum Boss2Action
    {
        Burrow,
        Melee,
        Projectile,
    }

    public enum Boss2BurrowState
    {
        AboveGround,
        Entering,
        HiddenMoving,
        Emerging,
    }

    public static class Boss2ActionSelector
    {
        // 根据当前规则选择本体。
        public static Boss2Action SelectHead(float distance, bool facingAway, float sample, Boss2ActionRules rules)
        {
            Validate(distance, sample);
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (distance > rules.HeadFarDistance || facingAway)
            {
                return sample < rules.HeadBurrowWeight ? Boss2Action.Burrow : Boss2Action.Projectile;
            }
            return distance < rules.HeadMeleeDistance ? Boss2Action.Melee : Boss2Action.Projectile;
        }

        // 根据当前规则选择尾部。
        public static Boss2Action SelectTail(float distance, bool facingAway, float sample, Boss2ActionRules rules)
        {
            Validate(distance, sample);
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (distance > rules.TailFarDistance || facingAway)
            {
                return Boss2Action.Burrow;
            }
            return sample < rules.TailMeleeWeight ? Boss2Action.Melee : Boss2Action.Projectile;
        }

        private static void Validate(float distance, float sample)
        {
            if (distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }
            if (sample < 0f || sample >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(sample));
            }
        }
    }

    public sealed class Boss2PhaseCoordinator
    {
        private int previousRemaining;
        private bool tailActivated;

        // 初始化Boss2Phase协调器实例及其核心依赖。
        public Boss2PhaseCoordinator(int initialRemaining, int activationRemaining)
        {
            if (initialRemaining <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialRemaining));
            }
            previousRemaining = initialRemaining;
            if (activationRemaining < 0 || activationRemaining >= initialRemaining)
                throw new ArgumentOutOfRangeException(nameof(activationRemaining));
            this.activationRemaining = activationRemaining;
        }

        private readonly int activationRemaining;

        public bool IsTailActive => tailActivated;

        // 执行观察剩余数量对应的主要流程。
        public bool ObserveRemaining(int remaining)
        {
            if (remaining < 0 || remaining > previousRemaining)
            {
                throw new ArgumentOutOfRangeException(nameof(remaining));
            }

            var activate = !tailActivated && previousRemaining > activationRemaining && remaining <= activationRemaining;
            previousRemaining = remaining;
            if (activate)
            {
                tailActivated = true;
            }
            return activate;
        }
    }

    public sealed class Boss2BurrowFlow
    {
        public Boss2BurrowState State { get; private set; } = Boss2BurrowState.AboveGround;

        // 执行开始进入阶段对应的主要流程。
        public bool BeginEntering()
        {
            if (State != Boss2BurrowState.AboveGround)
            {
                return false;
            }
            State = Boss2BurrowState.Entering;
            return true;
        }

        // 执行Enter隐藏状态Movement对应的主要流程。
        public bool EnterHiddenMovement()
        {
            if (State != Boss2BurrowState.Entering)
            {
                return false;
            }
            State = Boss2BurrowState.HiddenMoving;
            return true;
        }

        // 执行开始出现阶段对应的主要流程。
        public bool BeginEmerging()
        {
            if (State != Boss2BurrowState.HiddenMoving)
            {
                return false;
            }
            State = Boss2BurrowState.Emerging;
            return true;
        }

        // 执行完成出现阶段对应的主要流程。
        public bool CompleteEmerging()
        {
            if (State != Boss2BurrowState.Emerging)
            {
                return false;
            }
            State = Boss2BurrowState.AboveGround;
            return true;
        }

        // 执行Interrupt对应的主要流程。
        public void Interrupt()
        {
            State = Boss2BurrowState.AboveGround;
        }
    }
}
