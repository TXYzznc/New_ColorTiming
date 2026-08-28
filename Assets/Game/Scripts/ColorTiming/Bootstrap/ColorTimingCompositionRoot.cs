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
            soundService = inputHost.AddComponent<GfColorTimingSoundService>();
            soundService.Initialize(gameTime);
            transientEntities = new GfTransientEntityService(soundService);
            settings = new GfColorTimingSettings();
            uiService = new GfColorTimingUiService(gameTime, sceneFlow, settings, gameInput, soundService);
            sceneFlow.TransitionStarted += OnTransitionStarted;
            initialized = true;
        }

        // 绑定场景依赖或事件监听。
        internal void BindScene(Scene scene, ColorTimingSceneId sceneId)
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
                InstallBattleRuntime(scene, sceneId);
            }
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
            }
            sceneFlow.Dispose();
        }

        // 响应TransitionStarted回调，并更新本对象状态。
        private void OnTransitionStarted(ColorTimingSceneId scene)
        {
            // GF.Entity recycles hidden objects on its next update. Request cleanup before
            // the outgoing Unity scene starts unloading so scene-parented effects remain valid.
            transientEntities?.ReleaseAll();
            soundService?.ResetTrackedSounds();
        }

        private void InstallBattleRuntime(Scene scene, ColorTimingSceneId sceneId)
        {
            var anchors = scene.GetRootGameObjects()
                .Select(root => root.GetComponent<BattleSceneAnchors>())
                .SingleOrDefault(value => value != null);
            if (anchors == null)
                throw new InvalidOperationException($"Scene '{scene.path}' requires one BattleSceneAnchors root.");
            var host = new GameObject("BattleRuntimeContext (Clone)");
            SceneManager.MoveGameObjectToScene(host, scene);
            battleRuntime = host.AddComponent<BattleRuntimeContext>();
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
