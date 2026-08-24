using System;
using System.Linq;
using ColorTiming.Combat;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Presentation.Entities;
using ColorTiming.Presentation.Audio;
using ColorTiming.Presentation.UI;
using ColorTiming.Settings;
using ColorTiming.Input.Adapters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorTiming.Input
{
    public static class ColorTimingSceneInputBinder
    {
        public static void Bind(
            Scene scene,
            IGameInput input,
            IGameTime gameTime,
            ITransientEntityService transientEntities,
            IColorTimingSceneFlow sceneFlow,
            IColorTimingSettings settings,
            IColorTimingSoundService soundService,
            IColorTimingUiService uiService)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("The scene must be valid and loaded.", nameof(scene));
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (gameTime == null)
            {
                throw new ArgumentNullException(nameof(gameTime));
            }
            if (transientEntities == null)
            {
                throw new ArgumentNullException(nameof(transientEntities));
            }
            if (sceneFlow == null)
            {
                throw new ArgumentNullException(nameof(sceneFlow));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (soundService == null)
            {
                throw new ArgumentNullException(nameof(soundService));
            }
            if (uiService == null)
            {
                throw new ArgumentNullException(nameof(uiService));
            }

            var behaviours = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(behaviour => behaviour != null)
                .ToArray();

            foreach (var consumer in behaviours.OfType<IGameInputConsumer>())
            {
                consumer.BindGameInput(input);
            }

            foreach (var consumer in behaviours.OfType<IGameTimeConsumer>())
            {
                consumer.BindGameTime(gameTime);
            }

            foreach (var consumer in behaviours.OfType<ITransientEntityConsumer>())
            {
                consumer.BindTransientEntities(transientEntities);
            }

            foreach (var consumer in behaviours.OfType<IColorTimingSceneFlowConsumer>())
            {
                consumer.BindSceneFlow(sceneFlow);
            }

            foreach (var consumer in behaviours.OfType<IColorTimingSettingsConsumer>())
            {
                consumer.BindSettings(settings);
            }

            foreach (var consumer in behaviours.OfType<IColorTimingSoundConsumer>())
            {
                consumer.BindSoundService(soundService);
            }
            foreach (var consumer in behaviours.OfType<IColorTimingUiConsumer>())
            {
                consumer.BindUiService(uiService);
            }
            MigrateAutoAudio(scene, soundService);

            BindBattleResults(scene, behaviours);
            BindUiSound(scene, behaviours);
            BindPlayerDamageSignals(scene, behaviours);

            var pointerConsumers = behaviours.OfType<IGameplayPointerConsumer>().ToArray();
            var cameraConsumers = behaviours.OfType<IGameplayCameraConsumer>().ToArray();
            if (pointerConsumers.Length == 0 && cameraConsumers.Length == 0)
            {
                return;
            }

            var cameras = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .Where(camera => camera != null && camera.enabled)
                .ToArray();
            var gameplayCamera = cameras.FirstOrDefault(camera => camera.CompareTag("MainCamera"))
                ?? cameras.FirstOrDefault();
            if (gameplayCamera == null)
            {
                throw new InvalidOperationException($"Scene '{scene.path}' has pointer consumers but no enabled gameplay camera.");
            }

            var pointerWorld = new GameplayPointerWorldAdapter(() => gameplayCamera);
            foreach (var consumer in pointerConsumers)
            {
                consumer.BindGameplayPointer(pointerWorld);
            }
            foreach (var consumer in cameraConsumers)
            {
                consumer.BindGameplayCamera(gameplayCamera);
            }
        }

        private static void BindBattleResults(Scene scene, MonoBehaviour[] behaviours)
        {
            var consumers = behaviours.OfType<IBattleResultConsumer>().ToArray();
            if (consumers.Length == 0)
            {
                return;
            }

            var sinks = behaviours.OfType<IBattleResultSink>().ToArray();
            if (sinks.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene '{scene.path}' requires exactly one battle-result sink, found {sinks.Length}.");
            }

            foreach (var consumer in consumers)
            {
                consumer.BindBattleResultSink(sinks[0]);
            }
        }

        private static void BindUiSound(Scene scene, MonoBehaviour[] behaviours)
        {
            var consumers = behaviours.OfType<IUiSoundConsumer>().ToArray();
            if (consumers.Length == 0)
            {
                return;
            }

            var sinks = behaviours.OfType<IUiSoundSink>().ToArray();
            if (sinks.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene '{scene.path}' requires exactly one UI-sound sink, found {sinks.Length}.");
            }

            foreach (var consumer in consumers)
            {
                consumer.BindUiSound(sinks[0]);
            }
        }

        private static void BindPlayerDamageSignals(Scene scene, MonoBehaviour[] behaviours)
        {
            var consumers = behaviours.OfType<IPlayerDamageSignalConsumer>().ToArray();
            if (consumers.Length == 0)
            {
                return;
            }

            var signals = behaviours.OfType<IPlayerDamageSignal>().ToArray();
            if (signals.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene '{scene.path}' requires exactly one player-damage signal, found {signals.Length}.");
            }

            foreach (var consumer in consumers)
            {
                consumer.BindPlayerDamageSignal(signals[0]);
            }
        }

        private static void MigrateAutoAudio(Scene scene, IColorTimingSoundService soundService)
        {
            var sources = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AudioSource>(true));
            foreach (var source in sources)
            {
                if (source == null || !source.playOnAwake || source.clip == null)
                {
                    continue;
                }

                source.Stop();
                source.playOnAwake = false;
                var isAmbient = source.clip.name.StartsWith("amb_", StringComparison.OrdinalIgnoreCase);
                var isBgm = !isAmbient
                    && source.gameObject.name.IndexOf("BGM", StringComparison.OrdinalIgnoreCase) >= 0;
                soundService.Play(
                    source.clip,
                    isBgm ? ColorTimingSoundChannel.BGM : ColorTimingSoundChannel.Environment,
                    source.transform.position,
                    source.loop);
            }
        }
    }
}
