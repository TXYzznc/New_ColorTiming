// 文件职责：定义 Gameplay指针世界坐标 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Input。

using UnityEngine;

namespace ColorTiming.Input
{
    /// <summary>Converts semantic pointer screen coordinates into the active gameplay world.</summary>
    public interface IGameplayPointerWorld
    {
        // 执行Resolve对应的主要流程。
        Vector2 Resolve(Vector2 screenPosition);
    }
}
