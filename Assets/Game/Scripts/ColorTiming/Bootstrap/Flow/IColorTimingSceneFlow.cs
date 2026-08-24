using System;

namespace ColorTiming.Bootstrap.Flow
{
    public interface IColorTimingSceneFlow
    {
        event Action<ColorTimingSceneId> TransitionStarted;
        event Action<float> TransitionProgress;
        event Action<ColorTimingSceneId> SceneChanged;
        event Action<ColorTimingSceneId, string> TransitionFailed;

        bool HasCurrentScene { get; }
        ColorTimingSceneId CurrentScene { get; }
        bool IsTransitioning { get; }

        bool TryLoad(ColorTimingSceneId scene, bool forceReload = false);
    }

    public interface IColorTimingSceneFlowConsumer
    {
        void BindSceneFlow(IColorTimingSceneFlow sceneFlow);
    }
}
