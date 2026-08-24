using System;

namespace ColorTiming.Presentation.UI
{
    public interface IPlayerDamageSignal
    {
        event Action Damaged;
    }

    public interface IPlayerDamageSignalConsumer
    {
        void BindPlayerDamageSignal(IPlayerDamageSignal signal);
    }
}
