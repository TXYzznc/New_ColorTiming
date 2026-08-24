using System;

namespace ColorTiming.Combat
{
    public interface IGameTime
    {
        event Action<float> ScaleChanged;
        float EffectiveScale { get; }
        IDisposable Acquire(float scale);
        void Pulse(float scale, float unscaledSeconds);
        void Reset();
    }

    public interface IGameTimeConsumer
    {
        void BindGameTime(IGameTime gameTime);
    }
}
