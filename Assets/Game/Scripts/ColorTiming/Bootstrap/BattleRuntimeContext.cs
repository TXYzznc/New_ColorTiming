// 文件职责：在场景中建立并释放战斗会话及运行时依赖。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections;
using ColorTiming.Application.Battle;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Combat;
using ColorTiming.Infrastructure.Unity.Input;
using ColorTiming.Input;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.Actors;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.UI.Contracts;
using ColorTiming.Presentation.UI.Models;
using ColorTiming.Presentation.UI.Presenters;
using ColorTiming.Settings;
using UnityEngine;

namespace ColorTiming.Bootstrap
{
    /// <summary>Runtime-owned battle composition and deterministic disposal boundary.</summary>
    public sealed class BattleRuntimeContext : MonoBehaviour
    {
        BattleSession session;
        PlayerActorView hero;
        IColorTimingUiService ui;
        IColorTimingSceneFlow sceneFlow;
        Coroutine pendingTransition;
        Coroutine resourcePreparation;
        bool resultHandled;

        /// <summary>Reports the resource-preparation slice of the battle loading flow in [0, 1].</summary>
        public event Action<float> ResourcePreparationProgress;
        /// <summary>Raised only after the battle session is ready to accept gameplay input.</summary>
        public event Action ResourcePreparationCompleted;
        /// <summary>Raised when a required battle resource cannot be prepared.</summary>
        public event Action<string> ResourcePreparationFailed;

        public BattleSession Session => session;

        public void Initialize(
            BattleSceneAnchors anchors,
            ColorTimingSceneId sceneId,
            IGameInput input,
            IGameTime gameTime,
            ITransientEntityService entities,
            IColorTimingSceneFlow flow,
            IColorTimingSettings settings,
            IColorTimingSoundService sound,
            IColorTimingUiService uiService)
        {
            if (session != null || resourcePreparation != null) throw new InvalidOperationException("BattleRuntimeContext is already initialized.");
            if (anchors == null) throw new ArgumentNullException(nameof(anchors));
            anchors.Validate(sceneId == ColorTimingSceneId.Boss1);
            resourcePreparation = StartCoroutine(PrepareAndInitialize(
                anchors, sceneId, input, gameTime, entities, flow, settings, sound, uiService));
        }

        IEnumerator PrepareAndInitialize(
            BattleSceneAnchors anchors,
            ColorTimingSceneId sceneId,
            IGameInput input,
            IGameTime gameTime,
            ITransientEntityService entities,
            IColorTimingSceneFlow flow,
            IColorTimingSettings settings,
            IColorTimingSoundService sound,
            IColorTimingUiService uiService)
        {
            var loadContext = anchors.CreateLoadContext($"{sceneId}.Initial");
            var weapons = loadContext.RequiredWeapons;
            hero = anchors.Hero;
            Debug.Log($"[ColorTiming.BattleLoad] action=Prepare.Begin context={loadContext.Id} controllers={weapons.Count}", this);
            ResourcePreparationProgress?.Invoke(0f);
            hero.PreloadWeaponAnimations(weapons);
            var previousReadyCount = -1;
            while (!hero.AreWeaponAnimationsReady(weapons)
                   && !hero.HasWeaponAnimationPreloadFailure(weapons))
            {
                int readyCount = hero.GetReadyWeaponAnimationCount(weapons);
                if (readyCount != previousReadyCount)
                {
                    previousReadyCount = readyCount;
                    ResourcePreparationProgress?.Invoke(weapons.Count == 0 ? 1f : (float)readyCount / weapons.Count);
                }
                yield return null;
            }

            resourcePreparation = null;
            if (hero.HasWeaponAnimationPreloadFailure(weapons))
            {
                const string error = "Battle resource preparation failed: at least one Hero weapon controller could not load.";
                Debug.LogError(error, this);
                ResourcePreparationFailed?.Invoke(error);
                yield break;
            }

            sceneFlow = flow ?? throw new ArgumentNullException(nameof(flow));
            ui = uiService ?? throw new ArgumentNullException(nameof(uiService));
            session = new BattleSession(
                sceneId == ColorTimingSceneId.Boss1 ? BattleKind.Boss1 : BattleKind.Boss2,
                new UnityRandomSource());
            session.PresentationRequested += OnPresentationRequested;

            anchors.Hero.BindBattleSession(session);
            anchors.Boss1?.BindBattleSession(session);
            anchors.Boss2?.BindBattleSession(session);
            var pointer = new GameplayPointerWorldAdapter(() => anchors.GameplayCamera);
            BindExplicit(anchors, input, gameTime, entities, flow, settings, sound, uiService, pointer);
            StartSoundCues(anchors, sound);

            ui.ShowBattleHud(new BattleHudPresentation(session));
            ui.ShowBattleTutorial(session);
            ResourcePreparationProgress?.Invoke(1f);
            Debug.Log($"[ColorTiming.BattleLoad] action=Prepare.Completed context={loadContext.Id}", this);
            ResourcePreparationCompleted?.Invoke();
        }

        /// <summary>
        /// Prepares an incoming level or wave while retaining the currently active battle
        /// context. The caller owns its Loading presentation and may activate the new level
        /// only from <paramref name="completed"/>.
        /// </summary>
        public bool TryPrepareResourceContext(
            BattleLoadContext loadContext,
            Action<float> progress,
            Action completed,
            Action<string> failed)
        {
            if (loadContext == null) throw new ArgumentNullException(nameof(loadContext));
            if (session == null || resourcePreparation != null) return false;
            resourcePreparation = StartCoroutine(PrepareResourceContext(loadContext, progress, completed, failed));
            return true;
        }

        IEnumerator PrepareResourceContext(
            BattleLoadContext loadContext,
            Action<float> progress,
            Action completed,
            Action<string> failed)
        {
            var weapons = loadContext.RequiredWeapons;
            Debug.Log($"[ColorTiming.BattleLoad] action=Prepare.Begin context={loadContext.Id} controllers={weapons.Count}", this);
            progress?.Invoke(0f);
            if (hero == null)
            {
                resourcePreparation = null;
                failed?.Invoke($"Battle load context '{loadContext.Id}' has no Hero resource target.");
                yield break;
            }

            hero.PreloadWeaponAnimations(weapons);
            var previousReadyCount = -1;
            while (!hero.AreWeaponAnimationsReady(weapons) && !hero.HasWeaponAnimationPreloadFailure(weapons))
            {
                int readyCount = hero.GetReadyWeaponAnimationCount(weapons);
                if (readyCount != previousReadyCount)
                {
                    previousReadyCount = readyCount;
                    progress?.Invoke(weapons.Count == 0 ? 1f : (float)readyCount / weapons.Count);
                }
                yield return null;
            }

            resourcePreparation = null;
            if (hero.HasWeaponAnimationPreloadFailure(weapons))
            {
                failed?.Invoke($"Battle load context '{loadContext.Id}' could not load all Hero weapon controllers.");
                yield break;
            }
            progress?.Invoke(1f);
            Debug.Log($"[ColorTiming.BattleLoad] action=Prepare.Completed context={loadContext.Id}", this);
            completed?.Invoke();
        }


        // 执行Show对应的主要流程。
        void Show(BattlePresentationResult result)
        {
            if (resultHandled) return;
            resultHandled = true;
            if (result == BattlePresentationResult.Boss1Defeated)
            {
                pendingTransition = StartCoroutine(LoadBoss2AfterDelay());
                return;
            }
            ui.ShowBattleResult(result);
        }

        void BindExplicit(
            BattleSceneAnchors anchors,
            IGameInput input,
            IGameTime gameTime,
            ITransientEntityService entities,
            IColorTimingSceneFlow flow,
            IColorTimingSettings settings,
            IColorTimingSoundService sound,
            IColorTimingUiService uiService,
            IGameplayPointerWorld pointer)
        {
            var bindings = anchors.ExplicitBindings;
            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (binding is IGameInputConsumer inputConsumer) inputConsumer.BindGameInput(input);
                if (binding is IBattleSessionConsumer sessionConsumer) sessionConsumer.BindBattleSession(session);
                if (binding is IGameTimeConsumer timeConsumer) timeConsumer.BindGameTime(gameTime);
                if (binding is ITransientEntityConsumer entityConsumer) entityConsumer.BindTransientEntities(entities);
                if (binding is IColorTimingSceneFlowConsumer flowConsumer) flowConsumer.BindSceneFlow(flow);
                if (binding is IColorTimingSettingsConsumer settingsConsumer) settingsConsumer.BindSettings(settings);
                if (binding is IColorTimingSoundConsumer soundConsumer) soundConsumer.BindSoundService(sound);
                if (binding is IColorTimingUiConsumer uiConsumer) uiConsumer.BindUiService(uiService);
                if (binding is IGameplayPointerConsumer pointerConsumer) pointerConsumer.BindGameplayPointer(pointer);
                if (binding is IGameplayCameraConsumer cameraConsumer) cameraConsumer.BindGameplayCamera(anchors.GameplayCamera);
                if (binding is IPlayerDamageSignalConsumer damageConsumer) damageConsumer.BindPlayerDamageSignal(anchors.Hero);
                if (binding is IPlayerTargetConsumer targetConsumer) targetConsumer.BindPlayerTarget(anchors.Hero.transform);
            }
        }

        static void StartSoundCues(BattleSceneAnchors anchors, IColorTimingSoundService sound)
        {
            var cues = anchors.SoundCues;
            for (var i = 0; i < cues.Length; i++)
            {
                var cue = cues[i];
                if (cue.Clip == null) continue;
                sound.Play(cue.Clip, cue.Channel, cue.Position, cue.Loop);
            }
        }

        // 响应展示请求回调，并更新本对象状态。
        void OnPresentationRequested(BattlePresentationEvent message)
        {
            if (message.Kind == BattlePresentationEventKind.BattleWon)
                Show(session.Kind == BattleKind.Boss1
                    ? BattlePresentationResult.Boss1Defeated
                    : BattlePresentationResult.FinalVictory);
            else if (message.Kind == BattlePresentationEventKind.BattleLost)
                Show(BattlePresentationResult.PlayerDefeated);
        }

        // 加载Boss2AfterDelay，并处理完成或失败结果。
        IEnumerator LoadBoss2AfterDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            pendingTransition = null;
            sceneFlow.TryLoad(ColorTimingSceneId.Boss2);
        }

        // 组件销毁时释放订阅、句柄和运行时资源。
        void OnDestroy()
        {
            if (pendingTransition != null) StopCoroutine(pendingTransition);
            if (resourcePreparation != null) StopCoroutine(resourcePreparation);
            if (session != null)
            {
                session.PresentationRequested -= OnPresentationRequested;
                session.Dispose();
                session = null;
            }
        }

        sealed class UnityRandomSource : IRandomSource
        {
            // 执行Range对应的主要流程。
            public int Range(int minimumInclusive, int maximumExclusive) =>
                UnityEngine.Random.Range(minimumInclusive, maximumExclusive);
        }
    }
}
