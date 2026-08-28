// 文件职责：定义 时间缩放协调器，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTiming.Combat
{
    public sealed class TimeScaleCoordinator
    {
        private readonly Dictionary<int, float> requests = new Dictionary<int, float>();
        private int nextId;

        public float EffectiveScale { get; private set; } = 1f;
        public event Action<float> Changed;

        // 申请一个受控作用域，并返回用于释放的句柄。
        public IDisposable Acquire(float scale)
        {
            if (scale < 0f || scale > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            var id = ++nextId;
            requests.Add(id, scale);
            Recalculate();
            return new Releaser(this, id);
        }

        // 恢复组件的默认配置或初始运行状态。
        public void Reset()
        {
            if (requests.Count == 0)
            {
                return;
            }

            requests.Clear();
            Recalculate();
        }

        // 释放当前对象及其持有的临时资源。
        private void Release(int id)
        {
            if (requests.Remove(id))
            {
                Recalculate();
            }
        }

        private void Recalculate()
        {
            var next = requests.Count == 0 ? 1f : requests.Values.Min();
            if (Math.Abs(next - EffectiveScale) < 0.0001f)
            {
                return;
            }

            EffectiveScale = next;
            Changed?.Invoke(next);
        }

        private sealed class Releaser : IDisposable
        {
            private TimeScaleCoordinator owner;
            private readonly int id;

            // 初始化Releaser实例及其核心依赖。
            public Releaser(TimeScaleCoordinator owner, int id)
            {
                this.owner = owner;
                this.id = id;
            }

            // 释放本对象持有的订阅、服务和临时资源。
            public void Dispose()
            {
                var current = owner;
                owner = null;
                current?.Release(id);
            }
        }
    }
}
