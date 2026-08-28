// 文件职责：定义 游戏时间 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Application / Ports。

using System;

namespace ColorTiming.Combat
{
    public interface IGameTime
    {
        event Action<float> ScaleChanged;
        float EffectiveScale { get; }
        // 申请一个受控作用域，并返回用于释放的句柄。
        IDisposable Acquire(float scale);
        // 创建一次限时请求，并按持续时间自动结束。
        void Pulse(float scale, float unscaledSeconds);
        // 恢复组件的默认配置或初始运行状态。
        void Reset();
    }

    public interface IGameTimeConsumer
    {
        // 绑定游戏时间依赖或事件监听。
        void BindGameTime(IGameTime gameTime);
    }
}
