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
        private bool disposed;
        private float transitionProgress;

        public ColorTimingSceneFlow(Action<ColorTimingSceneId> beginTransition)
        {
            this.beginTransition = beginTransition ?? throw new ArgumentNullException(nameof(beginTransition));
        }

        public event Action<ColorTimingSceneId> TransitionStarted;
        public event Action<float> TransitionProgress;
        public event Action<ColorTimingSceneId> SceneChanged;
        public event Action<ColorTimingSceneId, string> TransitionFailed;

        public bool HasCurrentScene => hasCurrentScene;
        public ColorTimingSceneId CurrentScene => currentScene;
        public bool IsTransitioning => isTransitioning;

        public bool TryLoad(ColorTimingSceneId scene, bool forceReload = false)
        {
            ThrowIfDisposed();
            if (isTransitioning || (!forceReload && hasCurrentScene && currentScene == scene))
            {
                return false;
            }

            pendingScene = scene;
            isTransitioning = true;
            transitionProgress = 0f;
            TransitionStarted?.Invoke(scene);

            try
            {
                beginTransition(scene);
                return true;
            }
            catch
            {
                isTransitioning = false;
                throw;
            }
        }

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

        internal void FailTransition(ColorTimingSceneId scene, string errorMessage)
        {
            ThrowIfDisposed();
            EnsurePending(scene);
            isTransitioning = false;
            TransitionFailed?.Invoke(scene, errorMessage ?? string.Empty);
        }

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
