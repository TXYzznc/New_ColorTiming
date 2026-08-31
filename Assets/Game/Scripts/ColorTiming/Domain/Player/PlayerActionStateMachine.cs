// 文件职责：定义 玩家动作状态Machine，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Player。

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
        private readonly float hitInvulnerabilitySeconds;

        public PlayerActionStateMachine(float hitInvulnerabilitySeconds)
        {
            if (hitInvulnerabilitySeconds < 0f) throw new ArgumentOutOfRangeException(nameof(hitInvulnerabilitySeconds));
            this.hitInvulnerabilitySeconds = hitInvulnerabilitySeconds;
        }

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
        /// <summary>手动拾取/丢弃只在普通移动状态有效；攻击中的请求直接忽略且不排队。</summary>
        public bool CanInteractWithWeapons => State == PlayerActionState.Locomotion && !IsSkillMoving;
        public bool CanEvadeDamage => IsDashing && HasDashInvulnerability;
        public bool RejectsDamage => !IsAlive || HasAnimationInvulnerability || HitInvulnerabilityRemaining > 0f;

        // 按当前时间步推进核心状态，并发布必要的状态变化。
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            HitInvulnerabilityRemaining = Math.Max(0f, HitInvulnerabilityRemaining - deltaTime);
        }

        // 设置移动输入，并使后续流程使用最新状态。
        public void SetMove(float x, float y)
        {
            MoveX = x;
            MoveY = y;
            if (Math.Abs(x) > 0.0001f && !IsDashing && !IsAttacking && !IsSkillMoving)
            {
                FacingX = x < 0f ? -1 : 1;
            }
        }

        // 执行开始冲刺对应的主要流程。
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

        // 执行结束冲刺对应的主要流程。
        public void EndDash()
        {
            if (IsDashing)
            {
                State = PlayerActionState.Locomotion;
            }
            HasDashInvulnerability = false;
        }

        // 设置冲刺无敌状态，并使后续流程使用最新状态。
        public void SetDashInvulnerable(bool active)
        {
            // Animation Events may be delivered immediately before the Animator state-enter
            // callback. Preserve the authored window signal; CanEvadeDamage still requires
            // the state machine to have entered Dashing, so this cannot grant immunity alone.
            HasDashInvulnerability = active && IsAlive;
        }

        // 执行开始攻击对应的主要流程。
        public bool BeginAttack()
        {
            if (State != PlayerActionState.Locomotion || IsSkillMoving)
            {
                return false;
            }

            State = PlayerActionState.Attacking;
            return true;
        }

        // 执行结束攻击对应的主要流程。
        public void EndAttack()
        {
            if (IsAttacking)
            {
                State = PlayerActionState.Locomotion;
            }
        }

        // 执行开始Hit对应的主要流程。
        public void BeginHit()
        {
            if (!IsAlive)
            {
                return;
            }

            State = PlayerActionState.HitStun;
            IsSkillMoving = false;
            HitInvulnerabilityRemaining = hitInvulnerabilitySeconds;
        }

        // 执行结束Hit对应的主要流程。
        public void EndHit()
        {
            if (IsHitStunned)
            {
                State = PlayerActionState.Locomotion;
            }
        }

        // 设置技能移动状态，并使后续流程使用最新状态。
        public void SetSkillMoving(bool active)
        {
            if (IsAlive)
            {
                IsSkillMoving = active;
            }
        }

        // 设置动画无敌状态，并使后续流程使用最新状态。
        public void SetAnimationInvulnerable(bool active)
        {
            HasAnimationInvulnerability = active && IsAlive;
        }

        // 执行Kill对应的主要流程。
        public void Kill()
        {
            State = PlayerActionState.Dead;
            IsSkillMoving = false;
            HasDashInvulnerability = false;
            HasAnimationInvulnerability = false;
        }
    }
}
