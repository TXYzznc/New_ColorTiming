using UnityEngine;

namespace ColorTiming.Input
{
    /// <summary>Converts semantic pointer screen coordinates into the active gameplay world.</summary>
    public interface IGameplayPointerWorld
    {
        Vector2 Resolve(Vector2 screenPosition);
    }
}
