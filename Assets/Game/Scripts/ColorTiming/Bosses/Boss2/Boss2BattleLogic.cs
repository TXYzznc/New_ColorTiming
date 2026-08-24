using System;

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
        public static Boss2Action SelectHead(float distance, bool facingAway, float sample)
        {
            Validate(distance, sample);
            if (distance > 12f || facingAway)
            {
                return sample < 0.7f ? Boss2Action.Burrow : Boss2Action.Projectile;
            }
            return distance < 9f ? Boss2Action.Melee : Boss2Action.Projectile;
        }

        public static Boss2Action SelectTail(float distance, bool facingAway, float sample)
        {
            Validate(distance, sample);
            if (distance > 10f || facingAway)
            {
                return Boss2Action.Burrow;
            }
            return sample < 0.5f ? Boss2Action.Melee : Boss2Action.Projectile;
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

        public Boss2PhaseCoordinator(int initialRemaining)
        {
            if (initialRemaining <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialRemaining));
            }
            previousRemaining = initialRemaining;
        }

        public bool IsTailActive => tailActivated;

        public bool ObserveRemaining(int remaining)
        {
            if (remaining < 0 || remaining > previousRemaining)
            {
                throw new ArgumentOutOfRangeException(nameof(remaining));
            }

            var activate = !tailActivated && previousRemaining == 12 && remaining == 11;
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

        public bool BeginEntering()
        {
            if (State != Boss2BurrowState.AboveGround)
            {
                return false;
            }
            State = Boss2BurrowState.Entering;
            return true;
        }

        public bool EnterHiddenMovement()
        {
            if (State != Boss2BurrowState.Entering)
            {
                return false;
            }
            State = Boss2BurrowState.HiddenMoving;
            return true;
        }

        public bool BeginEmerging()
        {
            if (State != Boss2BurrowState.HiddenMoving)
            {
                return false;
            }
            State = Boss2BurrowState.Emerging;
            return true;
        }

        public bool CompleteEmerging()
        {
            if (State != Boss2BurrowState.Emerging)
            {
                return false;
            }
            State = Boss2BurrowState.AboveGround;
            return true;
        }

        public void Interrupt()
        {
            State = Boss2BurrowState.AboveGround;
        }
    }
}
