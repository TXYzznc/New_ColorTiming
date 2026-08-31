// 文件职责：使用 Mecanim 呈现玩家动作，并安全同步按武器拆分的 Animator Controller。
// 所属模块：ColorTiming / Presentation / Actors / Player / Animation。

using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Player;
using UnityEngine;

namespace ColorTiming.Presentation.PlayerAnimation
{
    /// <summary>
    /// 当前 Sprite/Mecanim 动画适配器。资源加载仍由组合根拥有，本类只接收已就绪 Controller。
    /// </summary>
    public sealed class MecanimPlayerAnimationDriver : IPlayerAnimationDriver
    {
        private const string IdleState = "Daiji";
        private const string MoveState = "Move";
        private const string MoveSpeedParameter = "moveSpeed";
        private const string DashTrigger = "Dash";
        private const string AttackTrigger = "Atk";
        private const string AttackHeldParameter = "Atk_x";
        private const string WeaponTypeParameter = "weaponType";
        private const string SwitchWeaponTrigger = "switchWeapon";
        private const string HitTrigger = "Hit";
        private const string DeathTrigger = "Death";

        private readonly Animator animator;
        private readonly Dictionary<WeaponIdentity, RuntimeAnimatorController> controllers =
            new Dictionary<WeaponIdentity, RuntimeAnimatorController>();
        private RuntimeAnimatorController activeController;
        private int requestVersion;

        public MecanimPlayerAnimationDriver(
            Animator animator,
            WeaponIdentity initialWeapon,
            RuntimeAnimatorController initialController)
        {
            this.animator = animator != null ? animator : throw new ArgumentNullException(nameof(animator));
            RequestedWeapon = initialWeapon;
            PresentedWeapon = initialWeapon;
            activeController = initialController;
            if (initialController != null)
            {
                controllers[initialWeapon] = initialController;
            }
        }

        public WeaponIdentity RequestedWeapon { get; private set; }
        public WeaponIdentity PresentedWeapon { get; private set; }
        public bool HasPendingWeaponPresentation { get; private set; }
        public RuntimeAnimatorController ActiveController => activeController;
        public int RequestVersion => requestVersion;

        public bool IsInLocomotionState
        {
            get
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName(IdleState) || state.IsName(MoveState);
            }
        }

        /// <summary>登记由资源上下文预加载完成的候选 Controller；不会在回调中直接 Rebind。</summary>
        public void RegisterController(WeaponIdentity weapon, RuntimeAnimatorController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            controllers[weapon] = controller;
        }

        /// <summary>记录最新业务武器请求；旧请求由递增版本自然失效。</summary>
        public void RequestWeapon(WeaponIdentity weapon)
        {
            RequestedWeapon = weapon;
            requestVersion++;
            HasPendingWeaponPresentation = !PresentedWeapon.Equals(weapon);
        }

        /// <summary>
        /// 仅在稳定 Locomotion 边界同步表现。移动输入不是阻塞条件，攻击、Dash、受击和技能位移是。
        /// </summary>
        public bool TrySynchronizeWeapon(
            PlayerActionState actionState,
            bool isSkillMoving,
            float movementMagnitude,
            float attackHeldValue)
        {
            if (!HasPendingWeaponPresentation || actionState != PlayerActionState.Locomotion || isSkillMoving
                || animator.IsInTransition(0) || !IsInLocomotionState
                || !controllers.TryGetValue(RequestedWeapon, out var controller))
            {
                return false;
            }

            var targetWeapon = RequestedWeapon;
            var installVersion = requestVersion;
            if (!ReferenceEquals(activeController, controller))
            {
                animator.runtimeAnimatorController = controller;
                animator.Rebind();
                activeController = controller;
            }

            if (installVersion != requestVersion || !RequestedWeapon.Equals(targetWeapon))
            {
                return false;
            }

            PresentedWeapon = targetWeapon;
            HasPendingWeaponPresentation = false;
            animator.SetFloat(MoveSpeedParameter, movementMagnitude * 10f);
            animator.SetFloat(AttackHeldParameter, attackHeldValue);
            ApplyLocomotionLayers(movementMagnitude > 0.0001f);
            animator.SetInteger(WeaponTypeParameter, PresentedWeapon.ToLegacyAnimatorIndex());
            animator.SetTrigger(SwitchWeaponTrigger);
            return true;
        }

        /// <summary>按已呈现武器统一维护层权重，避免业务武器先变化后重武器层永久残留。</summary>
        public void SetLocomotion(float appliedMovementMagnitude, bool hasMovementInput)
        {
            ApplyLocomotionLayers(hasMovementInput);
            animator.SetFloat(MoveSpeedParameter, appliedMovementMagnitude * 10f);
        }

        public void SetAttackHeld(float value) => animator.SetFloat(AttackHeldParameter, value);
        public void RequestAttack() => animator.SetTrigger(AttackTrigger);
        public void RequestDash() => animator.SetTrigger(DashTrigger);
        public void RequestHit() => animator.SetTrigger(HitTrigger);
        public void RequestDeath() => animator.SetTrigger(DeathTrigger);
        public void SetPlaybackSpeed(float speed) => animator.speed = speed;

        private void ApplyLocomotionLayers(bool hasMovementInput)
        {
            if (animator.layerCount < 2) return;
            var usesHeavyMovementLayer = hasMovementInput
                                         && (PresentedWeapon.Type == WeaponType.Hammer
                                             || PresentedWeapon.Type == WeaponType.Axe);
            animator.SetLayerWeight(0, usesHeavyMovementLayer ? 0f : 1f);
            animator.SetLayerWeight(1, usesHeavyMovementLayer ? 1f : 0f);
        }
    }
}
