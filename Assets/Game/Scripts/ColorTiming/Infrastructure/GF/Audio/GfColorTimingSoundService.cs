// 文件职责：通过 GF.Sound 播放、停止和管理业务音频。
// 所属模块：ColorTiming / Infrastructure / GF / Audio。

using System;
using System.Collections.Generic;
using ColorTiming.Bootstrap;
using ColorTiming.Combat;
using ColorTiming.Presentation.Audio;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace ColorTiming.Infrastructure.GF.Audio
{
    /// <summary>Framework sound boundary with gameplay-only pause semantics.</summary>
    public sealed class GfColorTimingSoundService : MonoBehaviour, IColorTimingSoundService
    {
        readonly HashSet<int> sceneSounds = new HashSet<int>();
        readonly HashSet<int> gameplaySounds = new HashSet<int>();
        readonly HashSet<int> loadedSounds = new HashSet<int>();
        readonly Dictionary<int, string> soundDescriptions = new Dictionary<int, string>();
        readonly List<int> staleGameplaySounds = new List<int>();
        IGameTime gameTime;
        bool gameplayPaused;
        bool eventsSubscribed;

        // 执行Initialize对应的主要流程。
        public void Initialize(IGameTime time)
        {
            if (gameTime != null) gameTime.ScaleChanged -= OnScaleChanged;
            gameTime = time ?? throw new ArgumentNullException(nameof(time));
            gameTime.ScaleChanged += OnScaleChanged;
            SubscribeSoundEvents();
            OnScaleChanged(gameTime.EffectiveScale);
        }

        // 启动当前配置的动画、音频或其他表现。
        public int Play(AudioClip clip, ColorTimingSoundChannel channel, Vector3 position, bool loop = false)
        {
            if (clip == null || global::GF.Sound == null)
            {
                return 0;
            }

            var relativeName = BuildRelativeName(clip.name, channel);
            var serialId = global::GF.Sound.PlaySound(
                ColorTimingResourceIds.Sound(relativeName),
                channel.ToString(),
                position,
                loop);
            if (serialId > 0)
            {
                sceneSounds.Add(serialId);
                soundDescriptions[serialId] = $"clip={clip.name} channel={channel} loop={loop}";
                Log.Info(
                    "[ColorTiming.Audio] action=PlayRequested result=Accepted serialId={0} clip={1} channel={2} loop={3}",
                    serialId,
                    clip.name,
                    channel,
                    loop);
                if (IsGameplay(channel))
                {
                    gameplaySounds.Add(serialId);
                    if (gameplayPaused)
                    {
                        ApplyToGameplaySound(serialId, global::GF.Sound.PauseSound);
                    }
                }
            }
            else
            {
                Log.Warning(
                    "[ColorTiming.Audio] action=PlayRequested result=Rejected clip={0} channel={1} loop={2}",
                    clip.name,
                    channel,
                    loop);
            }
            return serialId;
        }

        // 执行ResetTrackedSounds对应的主要流程。
        public void ResetTrackedSounds()
        {
            if (sceneSounds.Count > 0)
            {
                Log.Info("[ColorTiming.Audio] action=ResetTrackedSounds count={0}", sceneSounds.Count);
            }
            if (global::GF.Sound != null)
            {
                foreach (var serialId in sceneSounds)
                {
                    global::GF.Sound.StopSound(serialId);
                }
            }

            sceneSounds.Clear();
            gameplaySounds.Clear();
            loadedSounds.Clear();
            soundDescriptions.Clear();
        }

        // 执行Stop对应的主要流程。
        public void Stop(int serialId)
        {
            if (serialId <= 0)
            {
                return;
            }

            global::GF.Sound?.StopSound(serialId);
            if (soundDescriptions.TryGetValue(serialId, out var description))
            {
                Log.Info("[ColorTiming.Audio] action=Stop serialId={0} {1}", serialId, description);
            }
            sceneSounds.Remove(serialId);
            gameplaySounds.Remove(serialId);
            loadedSounds.Remove(serialId);
            soundDescriptions.Remove(serialId);
        }

        // 根据当前配置构建RelativeName。
        public static string BuildRelativeName(string clipName, ColorTimingSoundChannel channel)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                throw new ArgumentException("Audio clip name is required.", nameof(clipName));
            }

            var fileName = clipName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                ? clipName
                : $"{clipName}.wav";
            return channel == ColorTimingSoundChannel.BGM
                ? $"BGM/{fileName}"
                : fileName;
        }

        // 逐帧推进需要实时刷新的业务或表现状态。
        private void Update()
        {
            // A sound requested while paused can still be loading. Reapplying pause makes
            // it settle into the paused state as soon as its GF sound agent becomes ready.
            if (!gameplayPaused || global::GF.Sound == null) return;
            ApplyToGameplaySounds(global::GF.Sound.PauseSound);
        }

        // 响应缩放变化回调，并更新本对象状态。
        void OnScaleChanged(float scale)
        {
            var shouldPause = scale <= 0f;
            if (shouldPause == gameplayPaused) return;
            gameplayPaused = shouldPause;
            if (global::GF.Sound == null) return;
            ApplyToGameplaySounds(gameplayPaused ? global::GF.Sound.PauseSound : global::GF.Sound.ResumeSound);
        }

        // 把当前规则或配置应用到ToGameplaySounds。
        void ApplyToGameplaySounds(Action<int> operation)
        {
            staleGameplaySounds.Clear();
            foreach (var serialId in gameplaySounds)
            {
                ApplyToGameplaySound(serialId, operation);
            }
            foreach (var serialId in staleGameplaySounds)
            {
                gameplaySounds.Remove(serialId);
                sceneSounds.Remove(serialId);
                loadedSounds.Remove(serialId);
                soundDescriptions.Remove(serialId);
            }
            staleGameplaySounds.Clear();
        }

        // 把当前规则或配置应用到ToGameplay音效。
        void ApplyToGameplaySound(int serialId, Action<int> operation)
        {
            try
            {
                operation(serialId);
            }
            catch (GameFrameworkException)
            {
                // Loading sounds are not attached to an agent yet; Update retries them.
                // Completed one-shots no longer exist and must be pruned from tracking.
                if (global::GF.Sound == null || !global::GF.Sound.IsLoadingSound(serialId))
                {
                    staleGameplaySounds.Add(serialId);
                }
            }
        }

        static bool IsGameplay(ColorTimingSoundChannel channel)
        {
            return channel == ColorTimingSoundChannel.Player
                || channel == ColorTimingSoundChannel.Boss
                || channel == ColorTimingSoundChannel.Environment;
        }

        void SubscribeSoundEvents()
        {
            if (eventsSubscribed || global::GF.Event == null)
            {
                return;
            }

            global::GF.Event.Subscribe(PlaySoundSuccessEventArgs.EventId, OnPlaySoundSuccess);
            global::GF.Event.Subscribe(PlaySoundFailureEventArgs.EventId, OnPlaySoundFailure);
            eventsSubscribed = true;
        }

        void OnPlaySoundSuccess(object sender, GameEventArgs eventArgs)
        {
            var args = (PlaySoundSuccessEventArgs)eventArgs;
            if (!soundDescriptions.TryGetValue(args.SerialId, out var description))
            {
                return;
            }

            loadedSounds.Add(args.SerialId);
            Log.Info(
                "[ColorTiming.Audio] action=PlayStarted result=Success serialId={0} loadDuration={1:0.000}s {2}",
                args.SerialId,
                args.Duration,
                description);
        }

        void OnPlaySoundFailure(object sender, GameEventArgs eventArgs)
        {
            var args = (PlaySoundFailureEventArgs)eventArgs;
            if (!soundDescriptions.TryGetValue(args.SerialId, out var description))
            {
                return;
            }

            Log.Error(
                "[ColorTiming.Audio] action=PlayStarted result=Failure serialId={0} errorCode={1} error={2} {3}",
                args.SerialId,
                args.ErrorCode,
                args.ErrorMessage,
                description);
            sceneSounds.Remove(args.SerialId);
            gameplaySounds.Remove(args.SerialId);
            loadedSounds.Remove(args.SerialId);
            soundDescriptions.Remove(args.SerialId);
        }

        // 组件销毁时释放订阅、句柄和运行时资源。
        private void OnDestroy()
        {
            if (eventsSubscribed && global::GF.Event != null)
            {
                global::GF.Event.Unsubscribe(PlaySoundSuccessEventArgs.EventId, OnPlaySoundSuccess);
                global::GF.Event.Unsubscribe(PlaySoundFailureEventArgs.EventId, OnPlaySoundFailure);
                eventsSubscribed = false;
            }
            if (gameTime != null) gameTime.ScaleChanged -= OnScaleChanged;
            gameTime = null;
            ResetTrackedSounds();
            staleGameplaySounds.Clear();
        }
    }
}
