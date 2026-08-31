// 文件职责：定义玩家业务状态到具体动画技术之间的稳定语义边界。
// 所属模块：ColorTiming / Presentation / Actors / Player / Animation。

using ColorTiming.Combat;
using ColorTiming.Player;

namespace ColorTiming.Presentation.PlayerAnimation
{
    /// <summary>
    /// 玩家动画表现驱动。业务层只提交动作意图和武器状态，不感知 Animator 或 Spine API。
    /// </summary>
    public interface IPlayerAnimationDriver
    {
        WeaponIdentity RequestedWeapon { get; }
        WeaponIdentity PresentedWeapon { get; }
        bool HasPendingWeaponPresentation { get; }
        bool IsInLocomotionState { get; }

        void RequestWeapon(WeaponIdentity weapon);
        bool TrySynchronizeWeapon(PlayerActionState actionState, bool isSkillMoving, float movementMagnitude, float attackHeldValue);
        void SetLocomotion(float appliedMovementMagnitude, bool hasMovementInput);
        void SetAttackHeld(float value);
        void RequestAttack();
        void RequestDash();
        void RequestHit();
        void RequestDeath();
        void SetPlaybackSpeed(float speed);
    }
}
