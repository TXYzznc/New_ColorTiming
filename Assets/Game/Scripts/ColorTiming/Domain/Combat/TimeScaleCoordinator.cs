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

        public void Reset()
        {
            if (requests.Count == 0)
            {
                return;
            }

            requests.Clear();
            Recalculate();
        }

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

            public Releaser(TimeScaleCoordinator owner, int id)
            {
                this.owner = owner;
                this.id = id;
            }

            public void Dispose()
            {
                var current = owner;
                owner = null;
                current?.Release(id);
            }
        }
    }
}
