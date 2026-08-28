// 文件职责：定义 玩家攻击输入Gate，承担 玩家 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Player。

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

        // 按当前时间步推进核心状态，并发布必要的状态变化。
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            activeTime += deltaTime;
        }

        // 恢复组件的默认配置或初始运行状态。
        public void Reset()
        {
            activeTime = 0f;
        }

        // 执行ShouldTrigger对应的主要流程。
        public bool ShouldTrigger(bool attackPressed, bool attackAlreadyActive)
        {
            return IsReady && attackPressed && !attackAlreadyActive;
        }

        // 执行按住状态Animator值对应的主要流程。
        public float HeldAnimatorValue(bool attackHeld)
        {
            return IsReady && attackHeld ? 1f : 0f;
        }
    }
}
