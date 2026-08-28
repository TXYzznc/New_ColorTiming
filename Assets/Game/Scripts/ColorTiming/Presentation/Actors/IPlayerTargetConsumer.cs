using UnityEngine;

namespace ColorTiming.Presentation.Actors
{
    /// <summary>Explicit presentation-only binding for actors that aim or move toward the player.</summary>
    public interface IPlayerTargetConsumer
    {
        void BindPlayerTarget(Transform playerTarget);
    }
}
