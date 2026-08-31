// 文件职责：把 ColorTiming 业务启动流程接入 GF Procedure。
// 所属模块：ColorTiming / Bootstrap。

using ColorTiming.Bootstrap.Flow;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using UnityEngine.SceneManagement;

namespace ColorTiming.Bootstrap
{
    [Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName)]
    public sealed class ColorTimingStartupProcedure : ProcedureBase, IFrameworkStartupProcedure
    {
        private const float DefaultTransitionDuration = 2f;
        private const float MinimumTransitionDuration = 0.25f;
        private const float MaximumTransitionDuration = 20f;
        private const float DurationHistoryWeight = 0.35f;
        private const float SceneLoadProgressWeight = 0.55f;
        private const float ResourcePreparationWeight = 0.35f;

        private ColorTimingCompositionRoot compositionRoot;
        private ColorTimingSceneId loadingScene;
        private string loadingSceneAsset;
        private bool waitingForTargetUnload;
        private bool eventsSubscribed;
        private float transitionStartedRealtime;
        private float sceneLoadRequestedRealtime;
        private float lastSceneLoadProgress;
        private int sceneLoadProgressSampleCount;
        private int lastSceneProgressLogBucket;
        private float estimatedTransitionDuration;
        private int lastTimeSampleProgressLogBucket;
        private bool timeSampleProgressActive;

        // 响应Enter回调，并更新本对象状态。
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            SubscribeEvents();
            compositionRoot = new ColorTimingCompositionRoot(BeginSceneTransition);
            compositionRoot.Initialize();
            Log.Info("[ColorTiming.Startup] action=InitialScene.Request target=StartMenu");
            bool accepted = compositionRoot.SceneFlow.TryLoad(ColorTimingSceneId.StartMenu);
            Log.Info(
                "[ColorTiming.Startup] action=InitialScene.Request result={0} target=StartMenu hasCurrentScene={1} isTransitioning={2}",
                accepted ? "Accepted" : "Rejected",
                compositionRoot.SceneFlow.HasCurrentScene,
                compositionRoot.SceneFlow.IsTransitioning);
            if (!accepted)
            {
                Log.Error("ColorTiming failed to request its initial StartMenu scene.");
            }
        }

        // 响应Leave回调，并更新本对象状态。
        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UnsubscribeEvents();
            compositionRoot?.Dispose();
            compositionRoot = null;
            base.OnLeave(procedureOwner, isShutdown);
        }

        // 使用当前设备此前的真实加载记录，平滑更新 Loading 进度；不依赖 CPU、GPU 等无法预测实际加载耗时的硬件参数。
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!timeSampleProgressActive || compositionRoot == null)
            {
                return;
            }

            float elapsed = UnityEngine.Time.realtimeSinceStartup - transitionStartedRealtime;
            float sampledProgress = SceneLoadProgressWeight * UnityEngine.Mathf.Clamp01(elapsed / estimatedTransitionDuration);
            int progressBucket = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt(sampledProgress * 20f), 0, 19);
            if (progressBucket > lastTimeSampleProgressLogBucket)
            {
                lastTimeSampleProgressLogBucket = progressBucket;
                UnityEngine.Debug.LogFormat(
                    "[ColorTiming.SceneFlow] action=Loading.Progress result=Reported target={0} source=AdaptiveDuration elapsed={1:0.000}s estimatedDuration={2:0.000}s mappedProgress={3:0.###}",
                    loadingScene,
                    elapsed,
                    estimatedTransitionDuration,
                    sampledProgress);
            }
            compositionRoot.ReportSceneTransitionProgress(sampledProgress);
        }

        private void BeginSceneTransition(ColorTimingSceneId scene)
        {
            transitionStartedRealtime = UnityEngine.Time.realtimeSinceStartup;
            sceneLoadRequestedRealtime = -1f;
            lastSceneLoadProgress = 0f;
            sceneLoadProgressSampleCount = 0;
            lastSceneProgressLogBucket = -1;
            loadingScene = scene;
            loadingSceneAsset = UtilityBuiltin.AssetsPath.GetScenePath(scene.ToResourceName());
            estimatedTransitionDuration = GetEstimatedTransitionDuration(scene);
            lastTimeSampleProgressLogBucket = -1;
            timeSampleProgressActive = true;
            waitingForTargetUnload = GF.Scene.SceneIsUnloading(loadingSceneAsset);
            GFTrace.Info("ColorTiming", "Scene.Load.Begin", null,
                GFTrace.Data("scene", scene.ToString(), "asset", loadingSceneAsset));
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=Transition.Begin result=Started target={0} asset={1} targetAlreadyUnloading={2} realtime={3:0.000}",
                scene,
                loadingSceneAsset,
                waitingForTargetUnload,
                transitionStartedRealtime);

            GF.Sound.StopAllLoadingSounds();
            GF.Sound.StopAllLoadedSounds();
            GF.Entity.HideAllLoadingEntities();
            GF.Entity.HideAllLoadedEntities();

            string[] loadedScenes = GF.Scene.GetLoadedSceneAssetNames();
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=Transition.Prepare result=Completed target={0} elapsed={1:0.000}s loadedSceneCount={2}",
                loadingScene,
                UnityEngine.Time.realtimeSinceStartup - transitionStartedRealtime,
                loadedScenes.Length);
            for (int i = 0; i < loadedScenes.Length; i++)
            {
                if (loadedScenes[i] == loadingSceneAsset)
                {
                    waitingForTargetUnload = true;
                }
                GF.Scene.UnloadScene(loadedScenes[i], this);
            }

            GF.Base.ResetNormalGameSpeed();
            if (!waitingForTargetUnload)
            {
                LoadPendingScene();
            }
        }

        // 加载待处理场景，并处理完成或失败结果。
        private void LoadPendingScene()
        {
            sceneLoadRequestedRealtime = UnityEngine.Time.realtimeSinceStartup;
            lastSceneLoadProgress = 0f;
            sceneLoadProgressSampleCount = 0;
            lastSceneProgressLogBucket = -1;
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=SceneLoad.Request result=Started target={0} asset={1} waitingForTargetUnload={2} transitionElapsed={3:0.000}s",
                loadingScene,
                loadingSceneAsset,
                waitingForTargetUnload,
                sceneLoadRequestedRealtime - transitionStartedRealtime);
            GF.Scene.LoadScene(loadingSceneAsset, this);
        }

        private void SubscribeEvents()
        {
            if (eventsSubscribed)
            {
                return;
            }

            GF.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GF.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GF.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GF.Event.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            GF.Event.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);
            eventsSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!eventsSubscribed)
            {
                return;
            }

            eventsSubscribed = false;
            GF.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GF.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GF.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GF.Event.Unsubscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            GF.Event.Unsubscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);
        }

        // 响应Unload场景成功回调，并更新本对象状态。
        private void OnUnloadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            UnloadSceneSuccessEventArgs args = (UnloadSceneSuccessEventArgs)eventArgs;
            if (args.UserData != this)
            {
                return;
            }

            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=SceneUnload result=Completed sceneAsset={0} transitionElapsed={1:0.000}s",
                args.SceneAssetName,
                UnityEngine.Time.realtimeSinceStartup - transitionStartedRealtime);
            if (!waitingForTargetUnload || args.SceneAssetName != loadingSceneAsset)
            {
                return;
            }

            waitingForTargetUnload = false;
            LoadPendingScene();
        }

        // 响应Unload场景失败回调，并更新本对象状态。
        private void OnUnloadSceneFailure(object sender, GameEventArgs eventArgs)
        {
            UnloadSceneFailureEventArgs args = (UnloadSceneFailureEventArgs)eventArgs;
            if (!waitingForTargetUnload || args.SceneAssetName != loadingSceneAsset)
            {
                return;
            }

            waitingForTargetUnload = false;
            const string error = "The current scene could not be unloaded before a same-scene reload.";
            compositionRoot.FailSceneTransition(loadingScene, error);
            Log.Error("ColorTiming scene '{0}' could not unload for reload.", loadingScene);
        }

        // 响应Load场景Update回调，并更新本对象状态。
        private void OnLoadSceneUpdate(object sender, GameEventArgs eventArgs)
        {
            LoadSceneUpdateEventArgs args = (LoadSceneUpdateEventArgs)eventArgs;
            if (args.UserData != this)
            {
                return;
            }

            lastSceneLoadProgress = args.Progress;
            sceneLoadProgressSampleCount++;
            int progressBucket = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt(args.Progress * 20f), 0, 19);
            if (progressBucket > lastSceneProgressLogBucket)
            {
                lastSceneProgressLogBucket = progressBucket;
                UnityEngine.Debug.LogFormat(
                    "[ColorTiming.SceneFlow] action=SceneLoad.Progress result=Observed target={0} source=UnityAsyncOperation rawProgress={1:0.###} samples={2}",
                    loadingScene,
                    args.Progress,
                    sceneLoadProgressSampleCount);
            }
        }

        // 响应Load场景成功回调，并更新本对象状态。
        private void OnLoadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            LoadSceneSuccessEventArgs args = (LoadSceneSuccessEventArgs)eventArgs;
            if (args.UserData != this)
            {
                return;
            }

            Scene loadedScene = SceneManager.GetSceneByPath(args.SceneAssetName);
            float sceneLoadCompletedRealtime = UnityEngine.Time.realtimeSinceStartup;
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=SceneLoad.Success result=Completed target={0} asset={1} sceneValid={2} loadElapsed={3:0.000}s transitionElapsed={4:0.000}s rawProgress={5:0.###} samples={6}",
                loadingScene,
                args.SceneAssetName,
                loadedScene.IsValid(),
                sceneLoadCompletedRealtime - sceneLoadRequestedRealtime,
                sceneLoadCompletedRealtime - transitionStartedRealtime,
                lastSceneLoadProgress,
                sceneLoadProgressSampleCount);
            compositionRoot.BindScene(
                loadedScene,
                loadingScene,
                OnBattleResourcePreparationProgress,
                CompleteLoadedSceneTransition,
                FailBattleResourcePreparation);
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=SceneBind result=Completed target={0} bindElapsed={1:0.000}s transitionElapsed={2:0.000}s",
                loadingScene,
                UnityEngine.Time.realtimeSinceStartup - sceneLoadCompletedRealtime,
                UnityEngine.Time.realtimeSinceStartup - transitionStartedRealtime);
        }

        private void OnBattleResourcePreparationProgress(float preparationProgress)
        {
            float combinedProgress = SceneLoadProgressWeight
                + ResourcePreparationWeight * UnityEngine.Mathf.Clamp01(preparationProgress);
            compositionRoot.ReportSceneTransitionProgress(combinedProgress);
        }

        private void CompleteLoadedSceneTransition()
        {
            if (compositionRoot == null) return;
            compositionRoot.ReportSceneTransitionProgress(0.9f);
            UpdateEstimatedTransitionDuration();
            timeSampleProgressActive = false;
            compositionRoot.CompleteSceneTransition(loadingScene);
            GFTrace.Success("ColorTiming", "Scene.Load.Success", null,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", loadingSceneAsset));
        }

        private void FailBattleResourcePreparation(string error)
        {
            if (compositionRoot == null) return;
            timeSampleProgressActive = false;
            compositionRoot.FailSceneTransition(loadingScene, error);
            GFTrace.Failure("ColorTiming", "Scene.Preparation.Failure", error,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", loadingSceneAsset));
            Log.Error("ColorTiming scene '{0}' resource preparation failed: {1}", loadingScene, error);
            GameEntry.Shutdown(ShutdownType.Restart);
        }

        // 响应Load场景失败回调，并更新本对象状态。
        private void OnLoadSceneFailure(object sender, GameEventArgs eventArgs)
        {
            LoadSceneFailureEventArgs args = (LoadSceneFailureEventArgs)eventArgs;
            if (args.UserData != this)
            {
                return;
            }

            compositionRoot.FailSceneTransition(loadingScene, args.ErrorMessage);
            timeSampleProgressActive = false;
            GFTrace.Failure("ColorTiming", "Scene.Load.Failure", args.ErrorMessage,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", args.SceneAssetName));
            Log.Error("ColorTiming scene '{0}' failed to load: {1}", loadingScene, args.ErrorMessage);
            GameEntry.Shutdown(ShutdownType.Restart);
        }

        private float GetEstimatedTransitionDuration(ColorTimingSceneId scene)
        {
            string key = $"ColorTiming.Loading.EstimatedDuration.{scene}";
            return UnityEngine.Mathf.Clamp(global::GF.Setting.GetFloat(key, DefaultTransitionDuration),
                MinimumTransitionDuration,
                MaximumTransitionDuration);
        }

        private void UpdateEstimatedTransitionDuration()
        {
            float observedDuration = UnityEngine.Time.realtimeSinceStartup - transitionStartedRealtime;
            float updatedDuration = UnityEngine.Mathf.Lerp(estimatedTransitionDuration, observedDuration, DurationHistoryWeight);
            updatedDuration = UnityEngine.Mathf.Clamp(updatedDuration, MinimumTransitionDuration, MaximumTransitionDuration);
            string key = $"ColorTiming.Loading.EstimatedDuration.{loadingScene}";
            global::GF.Setting.SetFloat(key, updatedDuration);
            GFBuiltin.Setting.Save();
            UnityEngine.Debug.LogFormat(
                "[ColorTiming.SceneFlow] action=Loading.Duration result=Updated target={0} observedDuration={1:0.000}s previousEstimate={2:0.000}s updatedEstimate={3:0.000}s",
                loadingScene,
                observedDuration,
                estimatedTransitionDuration,
                updatedDuration);
        }
    }
}
