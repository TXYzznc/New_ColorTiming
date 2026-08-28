using System;
using System.Collections.Generic;
using ColorTiming.Bootstrap;
using ColorTiming.Combat;
using ColorTiming.Presentation.Audio;
using GameFramework;
using UnityEngine;

namespace ColorTiming.Infrastructure.GF.Audio
{
    /// <summary>Framework sound boundary with gameplay-only pause semantics.</summary>
    public sealed class GfColorTimingSoundService : MonoBehaviour, IColorTimingSoundService
    {
        readonly HashSet<int> sceneSounds = new HashSet<int>();
        readonly HashSet<int> gameplaySounds = new HashSet<int>();
        readonly List<int> staleGameplaySounds = new List<int>();
        IGameTime gameTime;
        bool gameplayPaused;

        public void Initialize(IGameTime time)
        {
            if (gameTime != null) gameTime.ScaleChanged -= OnScaleChanged;
            gameTime = time ?? throw new ArgumentNullException(nameof(time));
            gameTime.ScaleChanged += OnScaleChanged;
            OnScaleChanged(gameTime.EffectiveScale);
        }

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
                if (IsGameplay(channel))
                {
                    gameplaySounds.Add(serialId);
                    if (gameplayPaused)
                    {
                        ApplyToGameplaySound(serialId, global::GF.Sound.PauseSound);
                    }
                }
            }
            return serialId;
        }

        public void ResetTrackedSounds()
        {
            if (global::GF.Sound != null)
            {
                foreach (var serialId in sceneSounds)
                {
                    global::GF.Sound.StopSound(serialId);
                }
            }

            sceneSounds.Clear();
            gameplaySounds.Clear();
        }

        public void Stop(int serialId)
        {
            if (serialId <= 0)
            {
                return;
            }

            global::GF.Sound?.StopSound(serialId);
            sceneSounds.Remove(serialId);
            gameplaySounds.Remove(serialId);
        }

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

        private void Update()
        {
            // A sound requested while paused can still be loading. Reapplying pause makes
            // it settle into the paused state as soon as its GF sound agent becomes ready.
            if (!gameplayPaused || global::GF.Sound == null) return;
            ApplyToGameplaySounds(global::GF.Sound.PauseSound);
        }

        void OnScaleChanged(float scale)
        {
            var shouldPause = scale <= 0f;
            if (shouldPause == gameplayPaused) return;
            gameplayPaused = shouldPause;
            if (global::GF.Sound == null) return;
            ApplyToGameplaySounds(gameplayPaused ? global::GF.Sound.PauseSound : global::GF.Sound.ResumeSound);
        }

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
            }
            staleGameplaySounds.Clear();
        }

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

        private void OnDestroy()
        {
            if (gameTime != null) gameTime.ScaleChanged -= OnScaleChanged;
            gameTime = null;
            ResetTrackedSounds();
            staleGameplaySounds.Clear();
        }
    }
}
