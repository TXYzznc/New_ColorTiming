// 文件职责：定义 ColorTiming场景流程 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Bootstrap / Flow。

using System;

namespace ColorTiming.Bootstrap.Flow
{
    public interface IColorTimingSceneFlow
    {
        event Action<SceneTransitionContext> TransitionStarted;
        event Action<float> TransitionProgress;
        event Action<ColorTimingSceneId> SceneChanged;
        event Action<ColorTimingSceneId, string> TransitionFailed;

        bool HasCurrentScene { get; }
        ColorTimingSceneId CurrentScene { get; }
        bool IsTransitioning { get; }

        // 尝试Load，并通过返回值报告是否成功。
        bool TryLoad(ColorTimingSceneId scene, bool forceReload = false);
    }

    public interface IColorTimingSceneFlowConsumer
    {
        // 绑定场景流程依赖或事件监听。
        void BindSceneFlow(IColorTimingSceneFlow sceneFlow);
    }
}
