using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using UnityEngine;

namespace ColorTiming.Infrastructure.Unity.Time
{
    /// <summary>
    /// Applies composable time requests to Unity. Timed requests advance in real time,
    /// so they also finish while another request has paused gameplay completely.
    /// </summary>
    [DefaultExecutionOrder(-9000)]
    public sealed class UnityGameTimeAdapter : MonoBehaviour, IGameTime
    {
        private readonly TimeScaleCoordinator coordinator = new TimeScaleCoordinator();
        private readonly List<TimedRequest> timedRequests = new List<TimedRequest>(4);

        public float EffectiveScale => coordinator.EffectiveScale;
        public event Action<float> ScaleChanged;

        private void Awake()
        {
            coordinator.Changed += ApplyScale;
            ApplyScale(coordinator.EffectiveScale);
        }

        private void Update()
        {
            var deltaTime = UnityEngine.Time.unscaledDeltaTime;
            for (var i = timedRequests.Count - 1; i >= 0; i--)
            {
                var request = timedRequests[i];
                request.Remaining -= deltaTime;
                if (request.Remaining > 0f)
                {
                    continue;
                }

                request.Handle.Dispose();
                timedRequests.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            coordinator.Changed -= ApplyScale;
            Reset();
            UnityEngine.Time.timeScale = 1f;
        }

        public IDisposable Acquire(float scale)
        {
            return coordinator.Acquire(scale);
        }

        public void Pulse(float scale, float unscaledSeconds)
        {
            if (unscaledSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(unscaledSeconds));
            }

            timedRequests.Add(new TimedRequest(coordinator.Acquire(scale), unscaledSeconds));
        }

        public void Reset()
        {
            for (var i = timedRequests.Count - 1; i >= 0; i--)
            {
                timedRequests[i].Handle.Dispose();
            }
            timedRequests.Clear();
            coordinator.Reset();
        }

        private void ApplyScale(float scale)
        {
            UnityEngine.Time.timeScale = scale;
            ScaleChanged?.Invoke(scale);
        }

        private sealed class TimedRequest
        {
            public TimedRequest(IDisposable handle, float remaining)
            {
                Handle = handle;
                Remaining = remaining;
            }

            public IDisposable Handle { get; }
            public float Remaining { get; set; }
        }
    }
}
