// 文件职责：使用 Boss Cue Catalog 通过 GF.Sound 播放语义化 Boss 音效。
// 所属模块：ColorTiming / Presentation / Audio。

using System;
using System.Collections.Generic;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using UnityEngine;

namespace ColorTiming.Presentation.Audio
{
    public sealed class BossSoundView : MonoBehaviour, IColorTimingSoundConsumer, IColorTimingConfigurationConsumer
    {
        private readonly HashSet<BossSoundCueId> _cueLookup = new HashSet<BossSoundCueId>();
        private readonly Dictionary<string, BossSoundCueId> _animationLookup =
            new Dictionary<string, BossSoundCueId>(StringComparer.Ordinal);
        private IColorTimingSoundService _soundService;
        private IColorTimingConfiguration _configuration;
        private string _cuePrefix;
        private bool _cacheReady;

        public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cuePrefix = sceneId == ColorTimingSceneId.Boss1 ? "boss1." : "boss2.";
            _cacheReady = false;
            RebuildCache();
        }

        // 绑定声音服务，并在玩法开始前完成配置校验和运行时缓存构建。
        public void BindSoundService(IColorTimingSoundService service)
        {
            _soundService = service ?? throw new ArgumentNullException(nameof(service));
            if (!_cacheReady)
            {
                RebuildCache();
            }
        }

        public bool TryPlay(BossSoundCueId cue)
        {
            return TryPlay(cue, transform.position);
        }

        public bool TryPlay(BossSoundCueId cue, Vector3 position)
        {
            EnsureCache();
            if (_soundService == null || !_cueLookup.Contains(cue))
            {
                return false;
            }
            return _soundService.PlayCue(cue.Value, position) > 0;
        }

        public bool TryPlayAnimationCue(string animationEventKey)
        {
            return TryPlayAnimationCue(animationEventKey, transform.position);
        }

        public bool TryPlayAnimationCue(string animationEventKey, Vector3 position)
        {
            EnsureCache();
            return !string.IsNullOrEmpty(animationEventKey)
                   && _animationLookup.TryGetValue(animationEventKey, out var cue)
                   && TryPlay(cue, position);
        }

        private void EnsureCache()
        {
            if (!_cacheReady)
            {
                RebuildCache();
            }
        }

        private void RebuildCache()
        {
            if (_configuration == null || string.IsNullOrEmpty(_cuePrefix))
            {
                throw new InvalidOperationException($"BossSoundView '{name}' requires runtime configuration.");
            }
            _cueLookup.Clear();
            _animationLookup.Clear();
            var cues = _configuration.GetSoundCues(_cuePrefix);
            for (var i = 0; i < cues.Count; i++)
            {
                var row = cues[i];
                _cueLookup.Add(new BossSoundCueId(row.CueId));
                if (string.IsNullOrWhiteSpace(row.AnimationEventKey)) continue;
                if (!_animationLookup.TryAdd(row.AnimationEventKey, new BossSoundCueId(row.CueId)))
                    throw new InvalidOperationException($"Duplicate Boss animation sound key '{row.AnimationEventKey}'.");
            }
            _cacheReady = true;
        }

        internal void SetMappingsForTests(params (string CueId, string AnimationKey)[] mappings)
        {
            _cueLookup.Clear();
            _animationLookup.Clear();
            for (var i = 0; i < mappings.Length; i++)
            {
                var cue = new BossSoundCueId(mappings[i].CueId);
                if (!_cueLookup.Add(cue)) throw new InvalidOperationException($"Duplicate Boss sound cue '{cue}'.");
                if (!string.IsNullOrWhiteSpace(mappings[i].AnimationKey)
                    && !_animationLookup.TryAdd(mappings[i].AnimationKey, cue))
                    throw new InvalidOperationException($"Duplicate Boss animation sound key '{mappings[i].AnimationKey}'.");
            }
            _cacheReady = true;
        }
    }
}
