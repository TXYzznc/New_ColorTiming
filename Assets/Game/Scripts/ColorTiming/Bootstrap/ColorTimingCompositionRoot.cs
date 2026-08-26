using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Input.Adapters;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.Audio;
using ColorTiming.Settings;
using ColorTiming.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Bootstrap
{
    /// <summary>
    /// Owns project-level services for the lifetime of the ColorTiming procedure.
    /// Dependencies are exposed explicitly; this type is never a static service locator.
    /// </summary>
    public sealed class ColorTimingCompositionRoot : IDisposable
    {
        private readonly ColorTimingSceneFlow sceneFlow;
        private GameObject inputHost;
        private LegacyGameInputAdapter gameInput;
        private UnityGameTimeAdapter gameTime;
        private GfTransientEntityService transientEntities;
        private GfColorTimingSettings settings;
        private GfColorTimingSoundService soundService;
        private GfColorTimingUiService uiService;
        private bool initialized;
        private bool disposed;

        public ColorTimingCompositionRoot(Action<ColorTimingSceneId> beginSceneTransition)
        {
            sceneFlow = new ColorTimingSceneFlow(
                beginSceneTransition ?? throw new ArgumentNullException(nameof(beginSceneTransition)));
        }

        public IColorTimingSceneFlow SceneFlow => sceneFlow;
        public IGameInput GameInput => gameInput;
        public IGameTime GameTime => gameTime;

        public void Initialize()
        {
            ThrowIfDisposed();
            if (initialized)
            {
                return;
            }

            inputHost = new GameObject("[ColorTiming] Input (Clone)");
            UnityEngine.Object.DontDestroyOnLoad(inputHost);
            gameInput = inputHost.AddComponent<LegacyGameInputAdapter>();
            gameTime = inputHost.AddComponent<UnityGameTimeAdapter>();
            soundService = inputHost.AddComponent<GfColorTimingSoundService>();
            soundService.Initialize(gameTime);
            transientEntities = new GfTransientEntityService(soundService);
            settings = new GfColorTimingSettings();
            uiService = new GfColorTimingUiService(gameTime, sceneFlow, settings, gameInput, soundService);
            sceneFlow.TransitionStarted += OnTransitionStarted;
            initialized = true;
        }

        internal void BindScene(Scene scene, ColorTimingSceneId sceneId)
        {
            ThrowIfDisposed();
            if (!initialized || gameInput == null)
            {
                throw new InvalidOperationException("Composition root is not initialized.");
            }

            transientEntities.ReleaseAll();
            soundService.ResetTrackedSounds();
            ColorTimingUrpCameraStack.Configure(scene, sceneId);
            ColorTimingSceneInputBinder.Bind(
                scene, gameInput, gameTime, transientEntities, sceneFlow, settings, soundService, uiService);
            uiService.PresentScene(sceneId);
        }

        internal void CompleteSceneTransition(ColorTimingSceneId scene)
        {
            ThrowIfDisposed();
            sceneFlow.CompleteTransition(scene);
        }

        internal void ReportSceneTransitionProgress(float progress)
        {
            ThrowIfDisposed();
            sceneFlow.ReportTransitionProgress(progress);
        }

        internal void FailSceneTransition(ColorTimingSceneId scene, string errorMessage)
        {
            ThrowIfDisposed();
            sceneFlow.FailTransition(scene, errorMessage);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            initialized = false;
            sceneFlow.TransitionStarted -= OnTransitionStarted;
            ColorTimingUrpCameraStack.Reset();
            uiService?.Dispose();
            uiService = null;
            transientEntities?.ReleaseAll();
            if (inputHost != null)
            {
                UnityEngine.Object.Destroy(inputHost);
                inputHost = null;
                gameInput = null;
                gameTime = null;
                transientEntities = null;
                settings = null;
                soundService = null;
            }
            sceneFlow.Dispose();
        }

        private void OnTransitionStarted(ColorTimingSceneId scene)
        {
            // GF.Entity recycles hidden objects on its next update. Request cleanup before
            // the outgoing Unity scene starts unloading so scene-parented effects remain valid.
            transientEntities?.ReleaseAll();
            soundService?.ResetTrackedSounds();
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ColorTimingCompositionRoot));
            }
        }
    }
}
