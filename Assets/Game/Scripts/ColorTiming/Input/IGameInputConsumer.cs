// 文件职责：定义 Game输入消费者 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Input。

namespace ColorTiming.Input
{
    public interface IGameInputConsumer
    {
        // 绑定Game输入依赖或事件监听。
        void BindGameInput(IGameInput input);
    }

    public interface IGameplayPointerConsumer
    {
        // 绑定Gameplay指针依赖或事件监听。
        void BindGameplayPointer(IGameplayPointerWorld pointerWorld);
    }

    public interface IGameplayCameraConsumer
    {
        // 绑定Gameplay相机依赖或事件监听。
        void BindGameplayCamera(UnityEngine.Camera camera);
    }
}
