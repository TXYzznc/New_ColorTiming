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
        private ColorTimingCompositionRoot compositionRoot;
        private ColorTimingSceneId loadingScene;
        private string loadingSceneAsset;
        private bool waitingForTargetUnload;
        private bool eventsSubscribed;

        // 响应Enter回调，并更新本对象状态。
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            SubscribeEvents();
            compositionRoot = new ColorTimingCompositionRoot(BeginSceneTransition);
            compositionRoot.Initialize();
            if (!compositionRoot.SceneFlow.TryLoad(ColorTimingSceneId.StartMenu))
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

        private void BeginSceneTransition(ColorTimingSceneId scene)
        {
            loadingScene = scene;
            loadingSceneAsset = UtilityBuiltin.AssetsPath.GetScenePath(scene.ToResourceName());
            waitingForTargetUnload = GF.Scene.SceneIsUnloading(loadingSceneAsset);
            GFTrace.Info("ColorTiming", "Scene.Load.Begin", null,
                GFTrace.Data("scene", scene.ToString(), "asset", loadingSceneAsset));

            GF.Sound.StopAllLoadingSounds();
            GF.Sound.StopAllLoadedSounds();
            GF.Entity.HideAllLoadingEntities();
            GF.Entity.HideAllLoadedEntities();

            string[] loadedScenes = GF.Scene.GetLoadedSceneAssetNames();
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

            compositionRoot.ReportSceneTransitionProgress(args.Progress);
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
            compositionRoot.BindScene(loadedScene, loadingScene);
            compositionRoot.CompleteSceneTransition(loadingScene);
            GFTrace.Success("ColorTiming", "Scene.Load.Success", null,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", args.SceneAssetName));
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
            GFTrace.Failure("ColorTiming", "Scene.Load.Failure", args.ErrorMessage,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", args.SceneAssetName));
            Log.Error("ColorTiming scene '{0}' failed to load: {1}", loadingScene, args.ErrorMessage);
            GameEntry.Shutdown(ShutdownType.Restart);
        }
    }
}
