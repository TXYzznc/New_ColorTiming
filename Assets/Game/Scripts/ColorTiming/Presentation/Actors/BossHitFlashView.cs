// 文件职责：统一驱动一个或多个 Boss Renderer 的受击填充闪烁。
// 所属模块：ColorTiming / Presentation / Actors。

using System;
using ColorTiming.Bootstrap.Flow;
using ColorTiming.Configuration;
using UnityEngine;

namespace ColorTiming.Presentation.Actors
{
    public sealed class BossHitFlashView : MonoBehaviour, IColorTimingConfigurationConsumer
    {
        [SerializeField] private Renderer[] _renderers = Array.Empty<Renderer>();
        [SerializeField] private string _shaderProperty = "_FillPhase";
        private float _duration = 0.2f;
        private float _speed = 10f;

        private MaterialPropertyBlock _propertyBlock;
        private int _propertyId;
        private float _elapsed;

        public void BindConfiguration(IColorTimingConfiguration configuration, ColorTimingSceneId sceneId)
        {
            var battle = configuration.GetBattle(sceneId);
            var boss = configuration.GetBoss(battle.BossId);
            _duration = boss.HitFlashDuration;
            _speed = boss.HitFlashSpeed;
        }
        private float _remaining;
        private bool _initialized;

        private void Awake()
        {
            EnsureInitialized();
            enabled = false;
        }

        public void Play()
        {
            EnsureInitialized();
            ValidateRenderers();
            _elapsed = 0f;
            _remaining = _duration;
            Apply(0f);
            enabled = _duration > 0f;
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        internal void Advance(float deltaTime)
        {
            if (_remaining <= 0f)
            {
                return;
            }

            _elapsed += Mathf.Max(0f, deltaTime);
            _remaining -= Mathf.Max(0f, deltaTime);
            if (_remaining <= 0f)
            {
                _remaining = 0f;
                Apply(0f);
                enabled = false;
                return;
            }

            Apply(Mathf.PingPong(_elapsed * _speed, 1f));
        }

        private void Apply(float value)
        {
            for (var i = 0; i < _renderers.Length; i++)
            {
                var target = _renderers[i];
                if (target == null) continue;
                _propertyBlock.Clear();
                target.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_propertyId, value);
                target.SetPropertyBlock(_propertyBlock);
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            if (string.IsNullOrWhiteSpace(_shaderProperty))
            {
                throw new InvalidOperationException($"BossHitFlashView '{name}' requires a shader property name.");
            }

            _propertyBlock = new MaterialPropertyBlock();
            _propertyId = Shader.PropertyToID(_shaderProperty);
            _initialized = true;
        }

        private void ValidateRenderers()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                throw new InvalidOperationException($"BossHitFlashView '{name}' requires at least one Renderer.");
            }
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    throw new InvalidOperationException($"BossHitFlashView '{name}' renderer {i} is missing.");
                }
            }
        }

        internal void ConfigureForTests(Renderer[] renderers, float duration = 0.2f, float speed = 10f)
        {
            _renderers = renderers ?? Array.Empty<Renderer>();
            _duration = duration;
            _speed = speed;
        }
    }
}
