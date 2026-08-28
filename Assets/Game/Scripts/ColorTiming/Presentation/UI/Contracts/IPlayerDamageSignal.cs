using System;

namespace ColorTiming.Presentation.UI.Contracts
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
