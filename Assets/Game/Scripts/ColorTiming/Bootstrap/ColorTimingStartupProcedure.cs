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
        private bool eventsSubscribed;

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
            GFTrace.Info("ColorTiming", "Scene.Load.Begin", null,
                GFTrace.Data("scene", scene.ToString(), "asset", UtilityBuiltin.AssetsPath.GetScenePath(scene.ToResourceName())));

            GF.Sound.StopAllLoadingSounds();
            GF.Sound.StopAllLoadedSounds();
            GF.Entity.HideAllLoadingEntities();
            GF.Entity.HideAllLoadedEntities();

            string[] loadedScenes = GF.Scene.GetLoadedSceneAssetNames();
            for (int i = 0; i < loadedScenes.Length; i++)
            {
                GF.Scene.UnloadScene(loadedScenes[i]);
            }

            GF.Base.ResetNormalGameSpeed();
            GFBuiltin.BuiltinView?.ShowLoadingProgress(0f);
            GF.Scene.LoadScene(UtilityBuiltin.AssetsPath.GetScenePath(scene.ToResourceName()), this);
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
        }

        private void OnLoadSceneUpdate(object sender, GameEventArgs eventArgs)
        {
            LoadSceneUpdateEventArgs args = (LoadSceneUpdateEventArgs)eventArgs;
            if (args.UserData != this)
            {
                return;
            }

            compositionRoot.ReportSceneTransitionProgress(args.Progress);
            GFBuiltin.BuiltinView?.SetLoadingProgress(args.Progress);
        }

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
            GFBuiltin.BuiltinView?.HideLoadingProgress();
            GFTrace.Success("ColorTiming", "Scene.Load.Success", null,
                GFTrace.Data("scene", loadingScene.ToString(), "asset", args.SceneAssetName));
        }

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
