// 文件职责：定义 玩家伤害Signal 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Presentation / UI / Contracts。

using System;

namespace ColorTiming.Presentation.UI.Contracts
{
    public interface IPlayerDamageSignal
    {
        event Action Damaged;
    }

    public interface IPlayerDamageSignalConsumer
    {
        // 绑定玩家伤害Signal依赖或事件监听。
        void BindPlayerDamageSignal(IPlayerDamageSignal signal);
    }
}
