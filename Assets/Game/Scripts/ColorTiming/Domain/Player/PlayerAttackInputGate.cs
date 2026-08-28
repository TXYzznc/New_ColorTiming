using System;

namespace ColorTiming.Player
{
    /// <summary>
    /// Preserves the legacy pause-resume guard and the Animator's held-attack contract.
    /// The Animator transitions use Atk_x greater/less than 0.1; runtime values remain 0 or 1.
    /// </summary>
    public sealed class PlayerAttackInputGate
    {
        public const float ResumeGuardSeconds = 0.2f;
        public const float HeldAnimatorThreshold = 0.1f;

        private float activeTime;

        public bool IsReady => activeTime > ResumeGuardSeconds;

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            activeTime += deltaTime;
        }

        public void Reset()
        {
            activeTime = 0f;
        }

        public bool ShouldTrigger(bool attackPressed, bool attackAlreadyActive)
        {
            return IsReady && attackPressed && !attackAlreadyActive;
        }

        public float HeldAnimatorValue(bool attackHeld)
        {
            return IsReady && attackHeld ? 1f : 0f;
        }
    }
}
