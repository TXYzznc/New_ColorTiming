// 文件职责：把 UnityGame时间 的具体实现适配到上层接口。
// 所属模块：ColorTiming / Infrastructure / Unity / Time。

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

        // 缓存本组件依赖，并完成不依赖外部服务的本地初始化。
        private void Awake()
        {
            coordinator.Changed += ApplyScale;
            ApplyScale(coordinator.EffectiveScale);
        }

        // 逐帧推进需要实时刷新的业务或表现状态。
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

        // 组件销毁时释放订阅、句柄和运行时资源。
        private void OnDestroy()
        {
            coordinator.Changed -= ApplyScale;
            Reset();
            UnityEngine.Time.timeScale = 1f;
        }

        // 申请一个受控作用域，并返回用于释放的句柄。
        public IDisposable Acquire(float scale)
        {
            return coordinator.Acquire(scale);
        }

        // 创建一次限时请求，并按持续时间自动结束。
        public void Pulse(float scale, float unscaledSeconds)
        {
            if (unscaledSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(unscaledSeconds));
            }

            timedRequests.Add(new TimedRequest(coordinator.Acquire(scale), unscaledSeconds));
        }

        // 恢复组件的默认配置或初始运行状态。
        public void Reset()
        {
            for (var i = timedRequests.Count - 1; i >= 0; i--)
            {
                timedRequests[i].Handle.Dispose();
            }
            timedRequests.Clear();
            coordinator.Reset();
        }

        // 把当前规则或配置应用到缩放。
        private void ApplyScale(float scale)
        {
            UnityEngine.Time.timeScale = scale;
            ScaleChanged?.Invoke(scale);
        }

        private sealed class TimedRequest
        {
            // 初始化Timed请求实例及其核心依赖。
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
