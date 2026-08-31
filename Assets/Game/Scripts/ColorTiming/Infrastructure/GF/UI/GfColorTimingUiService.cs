// 文件职责：通过 GF.UI 打开、关闭并跟踪 ColorTiming 业务表单。
// 所属模块：ColorTiming / Infrastructure / GF / UI。

using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Input;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Presentation.UI.Models;
using ColorTiming.Presentation.UI.Presenters;
using ColorTiming.Settings;
using UnityGameFramework.Runtime;

namespace ColorTiming.Infrastructure.GF.UI
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
        int battleHudFormId = -1;
        int battleTutorialFormId = -1;
        BattlePresentationResult pendingResult;
        BattleHudPresentation pendingBattleHud;
        ColorTiming.Application.Battle.BattleSession pendingTutorialSession;
        IColorTimingLoadingForm loadingForm;
        IUiSoundSink uiSound;
        float loadingProgress;
        int loadingProgressLogBucket = -1;
        bool loadingCompletionRequested;
        SceneTransitionContext pendingTransitionPresentation;
        bool transitionPresentationPending;
        bool disposed;

        public event Action<SceneTransitionContext> TransitionPresentationReady;

        // 初始化GfColorTimingUIService实例及其核心依赖。
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
            Log.Info("[ColorTiming.UIFlow] action=Service.Initialize result=Success");
        }

        public bool IsPauseOpen => pauseFormId >= 0;

        // 执行Toggle暂停对应的主要流程。
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
            pauseFormId = global::GF.UI.OpenUIForm(UIViews.PauseMenu, parameters);
            if (pauseFormId < 0)
            {
                ReleasePause();
                return false;
            }
            return true;
        }

        // 执行Present场景对应的主要流程。
        public void PresentScene(ColorTimingSceneId scene)
        {
            ThrowIfDisposed();
            Log.Info(
                "[ColorTiming.UIFlow] action=PresentScene scene={0} closeGameplayForms=True opensMainMenu={1}",
                scene,
                scene == ColorTimingSceneId.StartMenu);
            CloseTrackedGameplayForms();
            if (scene != ColorTimingSceneId.StartMenu)
            {
                return;
            }

            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnStartMenuOpened;
            parameters.CloseCallback = _ => startMenuFormId = -1;
            startMenuFormId = global::GF.UI.OpenUIForm(UIViews.MainMenu, parameters);
            Log.Info(
                "[ColorTiming.UIFlow] action=MainMenu.Open.Request result={0} serialId={1} loadingSerialId={2}",
                startMenuFormId >= 0 ? "Accepted" : "Rejected",
                startMenuFormId,
                loadingFormId);
            if (startMenuFormId < 0)
            {
                UnityEngine.Debug.LogError("Failed to open the ColorTiming start-menu GF.UI form.");
            }
        }

        // 显示战斗结果并同步当前数据。
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
            resultFormId = global::GF.UI.OpenUIForm(UIViews.BattleResult, parameters);
            if (resultFormId >= 0)
            {
                return true;
            }

            ReleaseResult();
            return false;
        }

        // 显示战斗Hud并同步当前数据。
        public bool ShowBattleHud(BattleHudPresentation presentation)
        {
            ThrowIfDisposed();
            if (presentation == null) throw new ArgumentNullException(nameof(presentation));

            CloseBattleHud();
            pendingBattleHud = presentation;
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnBattleHudOpened;
            parameters.CloseCallback = _ => ResetBattleHudState();
            battleHudFormId = global::GF.UI.OpenUIForm(UIViews.BattleHud, parameters);
            if (battleHudFormId >= 0)
            {
                return true;
            }

            ResetBattleHudState();
            UnityEngine.Debug.LogError("Failed to open the ColorTiming battle HUD GF.UI form.");
            return false;
        }

        // 显示战斗Tutorial并同步当前数据。
        public bool ShowBattleTutorial(ColorTiming.Application.Battle.BattleSession session)
        {
            ThrowIfDisposed();
            if (session == null) throw new ArgumentNullException(nameof(session));

            CloseBattleTutorial();
            pendingTutorialSession = session;
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnBattleTutorialOpened;
            parameters.CloseCallback = _ => ResetBattleTutorialState();
            battleTutorialFormId = global::GF.UI.OpenUIForm(UIViews.BattleTutorial, parameters);
            if (battleTutorialFormId >= 0)
            {
                return true;
            }

            ResetBattleTutorialState();
            UnityEngine.Debug.LogError("Failed to open the ColorTiming battle tutorial GF.UI form.");
            return false;
        }

        // 恢复组件的默认配置或初始运行状态。
        public void Reset()
        {
            if (disposed) return;
            CloseTrackedForms();
        }

        // 响应Start菜单Opened回调，并更新本对象状态。
        void OnStartMenuOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingStartMenuForm form)
            {
                form.BindRuntime(sceneFlow, settings, soundService);
                uiSound = form.UiSound;
                Log.Info(
                    "[ColorTiming.UIFlow] action=MainMenu.Open.Callback result=Success serialId={0} loadingSerialId={1}",
                    startMenuFormId,
                    loadingFormId);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming start-menu prefab must implement IColorTimingStartMenuForm.");
            CloseStartMenu();
        }

        // 响应战斗结果Opened回调，并更新本对象状态。
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

        // 响应战斗HudOpened回调，并更新本对象状态。
        void OnBattleHudOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingBattleHudForm form)
            {
                form.BindRuntime(gameInput, this, pendingBattleHud);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming battle HUD prefab must implement IColorTimingBattleHudForm.");
            CloseBattleHud();
        }

        // 响应战斗TutorialOpened回调，并更新本对象状态。
        void OnBattleTutorialOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingBattleTutorialForm form)
            {
                form.BindRuntime(pendingTutorialSession, gameInput, gameTime, settings);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming battle tutorial prefab must implement IColorTimingBattleTutorialForm.");
            CloseBattleTutorial();
        }

        // 响应暂停Opened回调，并更新本对象状态。
        void OnPauseOpened(UIFormLogic logic)
        {
            if (logic is IColorTimingPauseForm form)
            {
                form.BindRuntime(sceneFlow, settings, uiSound);
                return;
            }

            UnityEngine.Debug.LogError("ColorTiming pause prefab must implement IColorTimingPauseForm.");
            ClosePause();
        }

        // 关闭暂停并结束本次生命周期。
        void ClosePause()
        {
            var serialId = pauseFormId;
            pauseFormId = -1;
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
            ReleasePause();
        }

        // 关闭Start菜单并结束本次生命周期。
        void CloseStartMenu()
        {
            var serialId = startMenuFormId;
            startMenuFormId = -1;
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
        }

        // 关闭战斗结果并结束本次生命周期。
        void CloseBattleResult()
        {
            var serialId = resultFormId;
            resultFormId = -1;
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
            ReleaseResult();
        }

        // 关闭战斗Hud并结束本次生命周期。
        void CloseBattleHud()
        {
            var serialId = battleHudFormId;
            ResetBattleHudState();
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
        }

        // 关闭战斗Tutorial并结束本次生命周期。
        void CloseBattleTutorial()
        {
            var serialId = battleTutorialFormId;
            ResetBattleTutorialState();
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
        }

        void ResetBattleHudState()
        {
            battleHudFormId = -1;
            pendingBattleHud = null;
        }

        void ResetBattleTutorialState()
        {
            battleTutorialFormId = -1;
            pendingTutorialSession = null;
        }

        // 释放结果及其临时资源。
        void ReleaseResult()
        {
            resultFormId = -1;
            resultPauseLease?.Dispose();
            resultPauseLease = null;
        }

        // 关闭TrackedForms并结束本次生命周期。
        void CloseTrackedForms()
        {
            CloseTrackedGameplayForms();
            CloseLoading("CloseTrackedForms");
        }

        // 关闭TrackedGameplayForms并结束本次生命周期。
        void CloseTrackedGameplayForms()
        {
            ClosePause();
            CloseStartMenu();
            CloseBattleResult();
            CloseBattleHud();
            CloseBattleTutorial();
        }

        void BeginLoading()
        {
            CloseLoading("ReplaceExistingBeforeOpen");
            loadingProgress = 0f;
            loadingProgressLogBucket = 0;
            loadingCompletionRequested = false;
            Log.Info(
                "[ColorTiming.UIFlow] action=Loading.Open.Request reason=SceneTransition hasCurrentScene={0} currentScene={1} isTransitioning={2}",
                sceneFlow.HasCurrentScene,
                sceneFlow.CurrentScene,
                sceneFlow.IsTransitioning);
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = OnLoadingOpened;
            parameters.CloseCallback = _ => ResetLoadingState();
            loadingFormId = global::GF.UI.OpenUIForm(UIViews.Loading, parameters);
            Log.Info(
                "[ColorTiming.UIFlow] action=Loading.Open.Request result={0} serialId={1}",
                loadingFormId >= 0 ? "Accepted" : "Rejected",
                loadingFormId);
            if (loadingFormId < 0)
            {
                UnityEngine.Debug.LogError("Failed to open the ColorTiming loading GF.UI form.");
                SignalTransitionPresentationReady("LoadingOpenRejected");
            }
        }

        // 响应加载Opened回调，并更新本对象状态。
        void OnLoadingOpened(UIFormLogic logic)
        {
            if (logic is not IColorTimingLoadingForm form)
            {
                UnityEngine.Debug.LogError("ColorTiming loading prefab must implement IColorTimingLoadingForm.");
                CloseLoading("InvalidLoadingForm");
                SignalTransitionPresentationReady("InvalidLoadingForm");
                return;
            }

            loadingForm = form;
            Log.Info(
                "[ColorTiming.UIFlow] action=Loading.Open.Callback result=Success serialId={0} progress={1:0.###} completionRequested={2}",
                loadingFormId,
                loadingProgress,
                loadingCompletionRequested);
            loadingForm.SetProgress(loadingProgress);
            SignalTransitionPresentationReady("LoadingFormOpened");
            if (loadingCompletionRequested)
            {
                loadingForm.CompleteAndClose();
            }
        }

        // 关闭加载并结束本次生命周期。
        void CloseLoading(string reason = "Unspecified")
        {
            var serialId = loadingFormId;
            if (serialId >= 0 || loadingForm != null)
            {
                Log.Info(
                    "[ColorTiming.UIFlow] action=Loading.Close.Request reason={0} serialId={1} hasBoundForm={2} progress={3:0.###}",
                    reason,
                    serialId,
                    loadingForm != null,
                    loadingProgress);
            }
            ResetLoadingState();
            if (serialId >= 0 && global::GF.UI != null)
            {
                global::GF.UI.CloseUIForm(serialId);
            }
        }

        void ResetLoadingState()
        {
            loadingFormId = -1;
            loadingForm = null;
            loadingProgress = 0f;
            loadingProgressLogBucket = -1;
            loadingCompletionRequested = false;
        }

        // 释放暂停及其临时资源。
        void ReleasePause()
        {
            pauseMenuLease?.Dispose();
            pauseMenuLease = null;
            pauseFormId = -1;
        }

        // 释放本对象持有的订阅、服务和临时资源。
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

        // 响应TransitionStarted回调，并更新本对象状态。
        void OnTransitionStarted(SceneTransitionContext context)
        {
            bool shouldPresentLoading = ShouldPresentLoading(context);
            Log.Info(
                "[ColorTiming.UIFlow] action=TransitionStarted source={0} target={1} initial={2} decision={3}",
                context.SourceScene.HasValue ? context.SourceScene.Value.ToString() : "None",
                context.TargetScene,
                context.IsInitialTransition,
                shouldPresentLoading ? "OpenLoading" : "SkipLoading.InitialStartMenu");
            CloseTrackedGameplayForms();
            pendingTransitionPresentation = context;
            transitionPresentationPending = true;
            if (shouldPresentLoading)
            {
                BeginLoading();
                return;
            }
            SignalTransitionPresentationReady("InitialStartMenuSkipsLoading");
        }

        // 在 Loading 已打开或允许降级时通知场景流开始实际切换。
        void SignalTransitionPresentationReady(string reason)
        {
            if (!transitionPresentationPending)
            {
                return;
            }

            transitionPresentationPending = false;
            Log.Info(
                "[ColorTiming.UIFlow] action=Transition.PresentationReady reason={0} target={1} frame={2} realtime={3:0.000}",
                reason,
                pendingTransitionPresentation.TargetScene,
                UnityEngine.Time.frameCount,
                UnityEngine.Time.realtimeSinceStartup);
            TransitionPresentationReady?.Invoke(pendingTransitionPresentation);
        }

        // 根据首场景例外判定本次转换是否需要显示项目 Loading 表单。
        internal static bool ShouldPresentLoading(SceneTransitionContext context)
        {
            return !context.IsInitialTransition || context.TargetScene != ColorTimingSceneId.StartMenu;
        }

        // 响应Transition进度回调，并更新本对象状态。
        void OnTransitionProgress(float progress)
        {
            loadingProgress = progress;
            loadingForm?.SetProgress(progress);
            int bucket = Math.Min(4, Math.Max(0, (int)(progress * 4f)));
            if (bucket > loadingProgressLogBucket)
            {
                loadingProgressLogBucket = bucket;
                Log.Info(
                    "[ColorTiming.UIFlow] action=Loading.Progress bucket={0} progress={1:0.###} serialId={2} hasBoundForm={3}",
                    bucket,
                    progress,
                    loadingFormId,
                    loadingForm != null);
            }
        }

        // 响应场景变化回调，并更新本对象状态。
        void OnSceneChanged(ColorTimingSceneId scene)
        {
            Log.Info(
                "[ColorTiming.UIFlow] action=SceneChanged scene={0} decision=CompleteLoading serialId={1} hasBoundForm={2}",
                scene,
                loadingFormId,
                loadingForm != null);
            loadingProgress = 1f;
            loadingCompletionRequested = true;
            loadingForm?.SetProgress(loadingProgress);
            loadingForm?.CompleteAndClose();
        }

        // 响应TransitionFailed回调，并更新本对象状态。
        void OnTransitionFailed(ColorTimingSceneId scene, string error)
        {
            Log.Warning(
                "[ColorTiming.UIFlow] action=TransitionFailed scene={0} error={1}",
                scene,
                error ?? string.Empty);
            CloseLoading("TransitionFailed");
        }

        void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(GfColorTimingUiService));
        }
    }
}
