// 文件职责：定义 ColorTiming场景流程，承担 流程 模块中的对应职责。
// 所属模块：ColorTiming / Bootstrap / Flow。

using System;

namespace ColorTiming.Bootstrap.Flow
{
    internal sealed class ColorTimingSceneFlow : IColorTimingSceneFlow, IDisposable
    {
        private readonly Action<ColorTimingSceneId> beginTransition;
        private ColorTimingSceneId currentScene;
        private ColorTimingSceneId pendingScene;
        private bool hasCurrentScene;
        private bool isTransitioning;
        private bool transitionDispatchRequested;
        private bool disposed;
        private float transitionProgress;

        // 初始化ColorTiming场景流程实例及其核心依赖。
        public ColorTimingSceneFlow(Action<ColorTimingSceneId> beginTransition)
        {
            this.beginTransition = beginTransition ?? throw new ArgumentNullException(nameof(beginTransition));
        }

        public event Action<SceneTransitionContext> TransitionStarted;
        public event Action<float> TransitionProgress;
        public event Action<ColorTimingSceneId> SceneChanged;
        public event Action<ColorTimingSceneId, string> TransitionFailed;

        public bool HasCurrentScene => hasCurrentScene;
        public ColorTimingSceneId CurrentScene => currentScene;
        public bool IsTransitioning => isTransitioning;

        // 尝试Load，并通过返回值报告是否成功。
        public bool TryLoad(ColorTimingSceneId scene, bool forceReload = false)
        {
            ThrowIfDisposed();
            if (isTransitioning || (!forceReload && hasCurrentScene && currentScene == scene))
            {
                return false;
            }

            pendingScene = scene;
            isTransitioning = true;
            transitionDispatchRequested = false;
            transitionProgress = 0f;
            var context = new SceneTransitionContext(hasCurrentScene ? currentScene : (ColorTimingSceneId?)null, scene);
            try
            {
                TransitionStarted?.Invoke(context);
                return true;
            }
            catch
            {
                isTransitioning = false;
                throw;
            }
        }

        // 在 Loading 表单已呈现后开始实际场景卸载与加载，避免 UI 打开异步操作被场景切换抢占。
        internal bool BeginPendingTransition()
        {
            ThrowIfDisposed();
            if (!isTransitioning || transitionDispatchRequested)
            {
                return false;
            }

            transitionDispatchRequested = true;
            try
            {
                beginTransition(pendingScene);
                return true;
            }
            catch
            {
                isTransitioning = false;
                transitionDispatchRequested = false;
                throw;
            }
        }

        // 执行完成Transition对应的主要流程。
        internal void CompleteTransition(ColorTimingSceneId scene)
        {
            ThrowIfDisposed();
            EnsurePending(scene);
            currentScene = scene;
            hasCurrentScene = true;
            isTransitioning = false;
            transitionProgress = 1f;
            TransitionProgress?.Invoke(1f);
            SceneChanged?.Invoke(scene);
        }

        // 执行ReportTransition进度对应的主要流程。
        internal void ReportTransitionProgress(float progress)
        {
            ThrowIfDisposed();
            if (!isTransitioning)
            {
                return;
            }

            var clamped = Math.Max(transitionProgress, Math.Min(1f, Math.Max(0f, progress)));
            if (clamped <= transitionProgress)
            {
                return;
            }
            transitionProgress = clamped;
            TransitionProgress?.Invoke(clamped);
        }

        // 执行失败Transition对应的主要流程。
        internal void FailTransition(ColorTimingSceneId scene, string errorMessage)
        {
            ThrowIfDisposed();
            EnsurePending(scene);
            isTransitioning = false;
            TransitionFailed?.Invoke(scene, errorMessage ?? string.Empty);
        }

        // 释放本对象持有的订阅、服务和临时资源。
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            isTransitioning = false;
            TransitionStarted = null;
            TransitionProgress = null;
            SceneChanged = null;
            TransitionFailed = null;
        }

        private void EnsurePending(ColorTimingSceneId scene)
        {
            if (!isTransitioning || pendingScene != scene)
            {
                throw new InvalidOperationException(
                    $"Scene transition completion does not match the pending scene. Pending='{pendingScene}', actual='{scene}'.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ColorTimingSceneFlow));
            }
        }
    }
}
