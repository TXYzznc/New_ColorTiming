using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Presentation.Audio;
using ColorTiming.Settings;
using UnityGameFramework.Runtime;

namespace ColorTiming.Presentation.UI
{
    /// <summary>Owns ColorTiming GF.UI forms and their cross-scene lifetime.</summary>
    public sealed class GfColorTimingUiService : IDisposable, IColorTimingUiService
    {
        readonly IGameTime gameTime;
        readonly IColorTimingSceneFlow sceneFlow;
        readonly IColorTimingSettings settings;
        readonly IGameInput gameInput;
        readonly IColorTimingSoundService soundService;
        IDisposable pauseMenuLease;
        IDisposable resultPauseLease;
        int pauseFormId = -1;
        int startMenuFormId = -1;
        int resultFormId = -1;
        int loadingFormId = -1;
        BattlePresentationResult pendingResult;
        IColorTimingLoadingForm loadingForm;
        float loadingProgress;
        bool loadingCompletionRequested;
        bool disposed;

        public GfColorTimingUiService(
            IGameTime gameTime,
            IColorTimingSceneFlow sceneFlow,
            IColorTimingSettings settings,
            IGameInput gameInput,
            IColorTimingSoundService soundService)
        {
            this.gameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
            this.sceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.gameInput = gameInput ?? throw new ArgumentNullException(nameof(gameInput));
            this.soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
            this.sceneFlow.TransitionStarted += OnTransitionStarted;
            this.sceneFlow.TransitionProgress += OnTransitionProgress;
            this.sceneFlow.SceneChanged += OnSceneChanged;
            this.sceneFlow.TransitionFailed += OnTransitionFailed;
        }

        public bool IsPauseOpen => pauseFormId >= 0;

        public bool TogglePause()
        {
            ThrowIfDisposed();
            if (pauseFormId >= 0)
            {
                ClosePause();
                return false;
            }

            pauseMenuLease = gameTime.Acquire(0f);
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnPauseOpened;
            parameters.CloseCallback = _ => ReleasePause();
            pauseFormId = GF.UI.OpenUIForm(UIViews.Esc, parameters);
            if (pauseFormId < 0)
            {
                ReleasePause();
                return false;
            }
            return true;
        }

        public void PresentScene(ColorTimingSceneId scene)
        {
            ThrowIfDisposed();
            CloseTrackedGameplayForms();
            if (scene != ColorTimingSceneId.StartMenu)
            {
                return;
            }

            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnStartMenuOpened;
            parameters.CloseCallback = _ => startMenuFormId = -1;
            startMenuFormId = GF.UI.OpenUIForm(UIViews.MainMenu, parameters);
            if (startMenuFormId < 0)
            {
                UnityEngine.Debug.LogError("Failed to open the ColorTiming start-menu GF.UI form.");
            }
        }

        public bool ShowBattleResult(BattlePresentationResult result)
        {
            ThrowIfDisposed();
            if (resultFormId >= 0)
            {
                return false;
            }

            pendingResult = result;
            resultPauseLease = gameTime.Acquire(0f);
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnBattleResultOpened;
            parameters.CloseCallback = _ => ReleaseResult();
            resultFormId = GF.UI.OpenUIForm(UIViews.BattleResult, parameters);
            if (resultFormId >= 0)
            {
                return true;
            }

            ReleaseResult();
            return false;
        }

        public void Reset()
        {
            if (disposed) return;
            CloseTrackedForms();
        }

        void OnStartMenuOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingStartMenuForm form)
            {
                form.BindRuntime(sceneFlow, settings, soundService);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming start-menu prefab must implement IColorTimingStartMenuForm.");
            CloseStartMenu();
        }

        void OnBattleResultOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingBattleResultForm form)
            {
                form.BindRuntime(sceneFlow, gameInput, pendingResult);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming battle-result prefab must implement IColorTimingBattleResultForm.");
            CloseBattleResult();
        }

        void OnPauseOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingPauseForm form)
            {
                form.BindRuntime(sceneFlow, settings);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming pause prefab must implement IColorTimingPauseForm.");
            ClosePause();
        }

        void ClosePause()
        {
            var serialId = pauseFormId;
            pauseFormId = -1;
            if (serialId >= 0 && GF.UI != null)
            {
                GF.UI.CloseUIForm(serialId);
            }
            ReleasePause();
        }

        void CloseStartMenu()
        {
            var serialId = startMenuFormId;
            startMenuFormId = -1;
            if (serialId >= 0 && GF.UI != null)
            {
                GF.UI.CloseUIForm(serialId);
            }
        }

        void CloseBattleResult()
        {
            var serialId = resultFormId;
            resultFormId = -1;
            if (serialId >= 0 && GF.UI != null)
            {
                GF.UI.CloseUIForm(serialId);
            }
            ReleaseResult();
        }

        void ReleaseResult()
        {
            resultFormId = -1;
            resultPauseLease?.Dispose();
            resultPauseLease = null;
        }

        void CloseTrackedForms()
        {
            CloseTrackedGameplayForms();
            CloseLoading();
        }

        void CloseTrackedGameplayForms()
        {
            ClosePause();
            CloseStartMenu();
            CloseBattleResult();
        }

        void BeginLoading()
        {
            CloseLoading();
            loadingProgress = 0f;
            loadingCompletionRequested = false;
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnLoadingOpened;
            parameters.CloseCallback = _ => ResetLoadingState();
            loadingFormId = GF.UI.OpenUIForm(UIViews.Loading, parameters);
            if (loadingFormId < 0)
            {
                UnityEngine.Debug.LogError("Failed to open the ColorTiming loading GF.UI form.");
            }
        }

        void OnLoadingOpened(UIFormLogic logic)
        {
            if (logic is not IColorTimingLoadingForm form)
            {
                UnityEngine.Debug.LogError("ColorTiming loading prefab must implement IColorTimingLoadingForm.");
                CloseLoading();
                return;
            }

            loadingForm = form;
            loadingForm.SetProgress(loadingProgress);
            if (loadingCompletionRequested)
            {
                loadingForm.CompleteAndClose();
            }
        }

        void CloseLoading()
        {
            var serialId = loadingFormId;
            ResetLoadingState();
            if (serialId >= 0 && GF.UI != null)
            {
                GF.UI.CloseUIForm(serialId);
            }
        }

        void ResetLoadingState()
        {
            loadingFormId = -1;
            loadingForm = null;
            loadingProgress = 0f;
            loadingCompletionRequested = false;
        }

        void ReleasePause()
        {
            pauseMenuLease?.Dispose();
            pauseMenuLease = null;
            pauseFormId = -1;
        }

        public void Dispose()
        {
            if (disposed) return;
            Reset();
            sceneFlow.TransitionStarted -= OnTransitionStarted;
            sceneFlow.TransitionProgress -= OnTransitionProgress;
            sceneFlow.SceneChanged -= OnSceneChanged;
            sceneFlow.TransitionFailed -= OnTransitionFailed;
            disposed = true;
        }

        void OnTransitionStarted(ColorTimingSceneId scene)
        {
            CloseTrackedGameplayForms();
            BeginLoading();
        }

        void OnTransitionProgress(float progress)
        {
            loadingProgress = progress;
            loadingForm?.SetProgress(progress);
        }

        void OnSceneChanged(ColorTimingSceneId scene)
        {
            loadingProgress = 1f;
            loadingCompletionRequested = true;
            loadingForm?.SetProgress(loadingProgress);
            loadingForm?.CompleteAndClose();
        }

        void OnTransitionFailed(ColorTimingSceneId scene, string error)
        {
            CloseLoading();
        }

        void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(GfColorTimingUiService));
        }
    }
}
