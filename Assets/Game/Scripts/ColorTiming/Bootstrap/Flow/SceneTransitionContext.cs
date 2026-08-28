// 文件职责：描述一次 ColorTiming 场景过渡的来源、目标和首次启动语义。
// 所属模块：ColorTiming / Bootstrap / Flow。

namespace ColorTiming.Bootstrap.Flow
{
    /// <summary>
    /// 场景流发布的不可变事实，订阅方据此决定清理和过渡表现，不再依赖事件触发时机推断。
    /// </summary>
    public readonly struct SceneTransitionContext
    {
        // 初始化场景过渡上下文。
        public SceneTransitionContext(ColorTimingSceneId? sourceScene, ColorTimingSceneId targetScene)
        {
            SourceScene = sourceScene;
            TargetScene = targetScene;
        }

        public ColorTimingSceneId? SourceScene { get; }
        public ColorTimingSceneId TargetScene { get; }
        public bool IsInitialTransition => !SourceScene.HasValue;
    }
}
