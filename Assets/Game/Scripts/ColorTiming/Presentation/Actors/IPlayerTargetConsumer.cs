// 文件职责：定义 玩家目标消费者 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / Actors。

using UnityEngine;

namespace ColorTiming.Presentation.Actors
{
    /// <summary>Explicit presentation-only binding for actors that aim or move toward the player.</summary>
    public interface IPlayerTargetConsumer
    {
        // 绑定玩家目标依赖或事件监听。
        void BindPlayerTarget(Transform playerTarget);
    }
}
