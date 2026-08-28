using System;

namespace ColorTiming.Player
{
    public enum PlayerActionState
    {
        Locomotion,
        Dashing,
        Attacking,
        HitStun,
        Dead,
    }

    /// <summary>
    /// Pure player action-state authority. Animator callbacks report transitions here;
    /// the Unity view reads the resulting movement and damage permissions.
    /// </summary>
    public sealed class PlayerActionStateMachine
    {
        private const float HitInvulnerabilitySeconds = 1f;

        public PlayerActionState State { get; private set; } = PlayerActionState.Locomotion;
        public int FacingX { get; private set; } = 1;
        public float MoveX { get; private set; }
        public float MoveY { get; private set; }
        public float DashY { get; private set; }
        public bool IsSkillMoving { get; private set; }
        public bool HasDashInvulnerability { get; private set; }
        public bool HasAnimationInvulnerability { get; private set; }
        public float HitInvulnerabilityRemaining { get; private set; }

        public bool IsAlive => State != PlayerActionState.Dead;
        public bool IsDashing => State == PlayerActionState.Dashing;
        public bool IsAttacking => State == PlayerActionState.Attacking;
        public bool IsHitStunned => State == PlayerActionState.HitStun;
        public bool CanMove => State == PlayerActionState.Locomotion && !IsSkillMoving;
        public bool CanAcceptCombatInput => IsAlive && !IsHitStunned;
        public bool CanEvadeDamage => IsDashing && HasDashInvulnerability;
        public bool RejectsDamage => !IsAlive || HasAnimationInvulnerability || HitInvulnerabilityRemaining > 0f;

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            HitInvulnerabilityRemaining = Math.Max(0f, HitInvulnerabilityRemaining - deltaTime);
        }

        public void SetMove(float x, float y)
        {
            MoveX = x;
            MoveY = y;
            if (Math.Abs(x) > 0.0001f && !IsDashing && !IsAttacking && !IsSkillMoving)
            {
                FacingX = x < 0f ? -1 : 1;
            }
        }

        public bool BeginDash()
        {
            if (State != PlayerActionState.Locomotion || IsSkillMoving)
            {
                return false;
            }

            State = PlayerActionState.Dashing;
            DashY = MoveY;
            return true;
        }

        public void EndDash()
        {
            if (IsDashing)
            {
                State = PlayerActionState.Locomotion;
            }
            HasDashInvulnerability = false;
        }

        public void SetDashInvulnerable(bool active)
        {
            // Animation Events may be delivered immediately before the Animator state-enter
            // callback. Preserve the authored window signal; CanEvadeDamage still requires
            // the state machine to have entered Dashing, so this cannot grant immunity alone.
            HasDashInvulnerability = active && IsAlive;
        }

        public bool BeginAttack()
        {
            if (State != PlayerActionState.Locomotion || IsSkillMoving)
            {
                return false;
            }

            State = PlayerActionState.Attacking;
            return true;
        }

        public void EndAttack()
        {
            if (IsAttacking)
            {
                State = PlayerActionState.Locomotion;
            }
        }

        public void BeginHit()
        {
            if (!IsAlive)
            {
                return;
            }

            State = PlayerActionState.HitStun;
            IsSkillMoving = false;
            HitInvulnerabilityRemaining = HitInvulnerabilitySeconds;
        }

        public void EndHit()
        {
            if (IsHitStunned)
            {
                State = PlayerActionState.Locomotion;
            }
        }

        public void SetSkillMoving(bool active)
        {
            if (IsAlive)
            {
                IsSkillMoving = active;
            }
        }

        public void SetAnimationInvulnerable(bool active)
        {
            HasAnimationInvulnerability = active && IsAlive;
        }

        public void Kill()
        {
            State = PlayerActionState.Dead;
            IsSkillMoving = false;
            HasDashInvulnerability = false;
            HasAnimationInvulnerability = false;
        }
    }
}
