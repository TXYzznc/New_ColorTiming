// 文件职责：集中创建、注入和释放 ColorTiming 运行时服务。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Linq;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Infrastructure.GF.Audio;
using ColorTiming.Infrastructure.GF.Entity;
using ColorTiming.Infrastructure.GF.Settings;
using ColorTiming.Infrastructure.GF.UI;
using ColorTiming.Infrastructure.Unity.Input;
using ColorTiming.Infrastructure.Unity.Time;
using ColorTiming.Input;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Camera;
using ColorTiming.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

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
        private ColorTimingTransitionScheduler transitionScheduler;
        private BattleRuntimeContext battleRuntime;
        private bool initialized;
        private bool disposed;

        // 初始化ColorTimingCompositionRoot实例及其核心依赖。
        public ColorTimingCompositionRoot(Action<ColorTimingSceneId> beginSceneTransition)
        {
            sceneFlow = new ColorTimingSceneFlow(
                beginSceneTransition ?? throw new ArgumentNullException(nameof(beginSceneTransition)));
        }

        public IColorTimingSceneFlow SceneFlow => sceneFlow;
        public IGameInput GameInput => gameInput;
        public IGameTime GameTime => gameTime;

        // 执行Initialize对应的主要流程。
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
            transitionScheduler = inputHost.AddComponent<ColorTimingTransitionScheduler>();
            soundService = inputHost.AddComponent<GfColorTimingSoundService>();
            soundService.Initialize(gameTime);
            transientEntities = new GfTransientEntityService(soundService);
            settings = new GfColorTimingSettings();
            uiService = new GfColorTimingUiService(gameTime, sceneFlow, settings, gameInput, soundService);
            uiService.TransitionPresentationReady += OnTransitionPresentationReady;
            sceneFlow.TransitionStarted += OnTransitionStarted;
            initialized = true;
        }

        // 绑定场景依赖或事件监听。
        internal void BindScene(
            Scene scene,
            ColorTimingSceneId sceneId,
            Action<float> reportPreparationProgress,
            Action completePreparation,
            Action<string> failPreparation)
        {
            ThrowIfDisposed();
            if (!initialized || gameInput == null)
            {
                throw new InvalidOperationException("Composition root is not initialized.");
            }

            transientEntities.ReleaseAll();
            soundService.ResetTrackedSounds();
            if (battleRuntime != null)
            {
                UnityEngine.Object.Destroy(battleRuntime.gameObject);
                battleRuntime = null;
            }
            ColorTimingUrpCameraStack.Configure(scene, sceneId);
            uiService.PresentScene(sceneId);
            if (sceneId == ColorTimingSceneId.Boss1 || sceneId == ColorTimingSceneId.Boss2)
            {
                InstallBattleRuntime(scene, sceneId, reportPreparationProgress, completePreparation, failPreparation);
                return;
            }

            reportPreparationProgress?.Invoke(1f);
            completePreparation?.Invoke();
        }

        // 执行完成场景Transition对应的主要流程。
        internal void CompleteSceneTransition(ColorTimingSceneId scene)
        {
            ThrowIfDisposed();
            sceneFlow.CompleteTransition(scene);
        }

        // 执行Report场景Transition进度对应的主要流程。
        internal void ReportSceneTransitionProgress(float progress)
        {
            ThrowIfDisposed();
            sceneFlow.ReportTransitionProgress(progress);
        }

        // 执行失败场景Transition对应的主要流程。
        internal void FailSceneTransition(ColorTimingSceneId scene, string errorMessage)
        {
            ThrowIfDisposed();
            sceneFlow.FailTransition(scene, errorMessage);
        }

        // 释放本对象持有的订阅、服务和临时资源。
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            initialized = false;
            sceneFlow.TransitionStarted -= OnTransitionStarted;
            if (uiService != null)
            {
                uiService.TransitionPresentationReady -= OnTransitionPresentationReady;
            }
            ColorTimingUrpCameraStack.Reset();
            uiService?.Dispose();
            uiService = null;
            if (battleRuntime != null)
            {
                UnityEngine.Object.Destroy(battleRuntime.gameObject);
                battleRuntime = null;
            }
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
                transitionScheduler = null;
            }
            sceneFlow.Dispose();
        }

        // 响应TransitionStarted回调，并更新本对象状态。
        private void OnTransitionStarted(SceneTransitionContext _)
        {
            // GF.Entity recycles hidden objects on its next update. Request cleanup before
            // the outgoing Unity scene starts unloading so scene-parented effects remain valid.
            transientEntities?.ReleaseAll();
            soundService?.ResetTrackedSounds();
        }

        // 在 Loading 表单可见后延迟派发场景切换，避免 UI 异步打开被立即卸载的场景抢占。
        private void OnTransitionPresentationReady(SceneTransitionContext context)
        {
            Log.Info(
                "[ColorTiming.SceneFlow] action=Transition.Dispatch.Schedule target={0} frame={1} realtime={2:0.000}",
                context.TargetScene,
                Time.frameCount,
                Time.realtimeSinceStartup);
            transitionScheduler?.Schedule(() =>
            {
                bool accepted = sceneFlow.BeginPendingTransition();
                Log.Info(
                    "[ColorTiming.SceneFlow] action=Transition.Dispatch.Begin target={0} result={1} frame={2} realtime={3:0.000}",
                    context.TargetScene,
                    accepted ? "Accepted" : "Ignored",
                    Time.frameCount,
                    Time.realtimeSinceStartup);
            });
        }

        private void InstallBattleRuntime(
            Scene scene,
            ColorTimingSceneId sceneId,
            Action<float> reportPreparationProgress,
            Action completePreparation,
            Action<string> failPreparation)
        {
            var anchors = scene.GetRootGameObjects()
                .Select(root => root.GetComponent<BattleSceneAnchors>())
                .SingleOrDefault(value => value != null);
            if (anchors == null)
                throw new InvalidOperationException($"Scene '{scene.path}' requires one BattleSceneAnchors root.");
            var host = new GameObject("BattleRuntimeContext (Clone)");
            SceneManager.MoveGameObjectToScene(host, scene);
            battleRuntime = host.AddComponent<BattleRuntimeContext>();
            battleRuntime.ResourcePreparationProgress += progress => reportPreparationProgress?.Invoke(progress);
            battleRuntime.ResourcePreparationCompleted += () => completePreparation?.Invoke();
            battleRuntime.ResourcePreparationFailed += error => failPreparation?.Invoke(error);
            battleRuntime.Initialize(
                anchors, sceneId, gameInput, gameTime, transientEntities,
                sceneFlow, settings, soundService, uiService);
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
