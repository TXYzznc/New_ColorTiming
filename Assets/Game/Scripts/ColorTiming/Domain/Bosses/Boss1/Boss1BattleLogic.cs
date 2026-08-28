using System;

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
        public Boss1Attack? LastAttack { get; private set; }

        public Boss1Attack Select(Boss1DistanceZone zone, float sample)
        {
            if (sample < 0f || sample >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(sample));
            }

            Boss1Attack selected;
            switch (zone)
            {
                case Boss1DistanceZone.Near:
                    selected = sample < 0.35f ? Boss1Attack.Attack2
                        : sample < 0.6f ? Boss1Attack.Attack1
                        : sample < 0.87f ? Boss1Attack.Attack3
                        : Boss1Attack.Attack4;
                    break;
                case Boss1DistanceZone.Middle:
                    selected = sample < 0.35f ? Boss1Attack.Attack6
                        : sample < 0.6f ? Boss1Attack.Attack1
                        : sample < 0.87f ? Boss1Attack.Attack3
                        : Boss1Attack.Attack4;
                    break;
                default:
                    selected = sample < 0.4f ? Boss1Attack.Attack3
                        : sample < 0.75f ? Boss1Attack.Attack4
                        : LastAttack != Boss1Attack.Attack5
                            ? Boss1Attack.Attack5
                            : Boss1Attack.Attack3;
                    break;
            }

            LastAttack = selected;
            return selected;
        }
    }

    public sealed class Boss1AttackCycle
    {
        public Boss1AttackCycle(float initialCooldown)
        {
            SetCooldown(initialCooldown);
        }

        public float CooldownRemaining { get; private set; }
        public bool IsAttacking { get; private set; }

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

        public bool BeginAttack()
        {
            if (IsAttacking || CooldownRemaining > 0f)
            {
                return false;
            }

            IsAttacking = true;
            return true;
        }

        public void CompleteAttack(float nextCooldown)
        {
            IsAttacking = false;
            SetCooldown(nextCooldown);
        }

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
