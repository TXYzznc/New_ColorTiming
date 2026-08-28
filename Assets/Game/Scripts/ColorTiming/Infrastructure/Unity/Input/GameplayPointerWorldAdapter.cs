// 文件职责：把屏幕指针坐标转换为指定平面的世界坐标。
// 所属模块：ColorTiming / Infrastructure / Unity / Input。

using System;
using ColorTiming.Input;
using UnityEngine;

namespace ColorTiming.Infrastructure.Unity.Input
{
    public sealed class GameplayPointerWorldAdapter : IGameplayPointerWorld
    {
        private readonly Func<Camera> cameraProvider;

        // 初始化Gameplay指针世界坐标Adapter实例及其核心依赖。
        public GameplayPointerWorldAdapter(Func<Camera> cameraProvider)
        {
            this.cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
        }

        // 执行Resolve对应的主要流程。
        public Vector2 Resolve(Vector2 screenPosition)
        {
            var camera = cameraProvider();
            if (camera == null)
            {
                throw new InvalidOperationException("The active gameplay camera is not available.");
            }

            var depth = Mathf.Abs(camera.transform.position.z);
            var world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            return new Vector2(world.x, world.y);
        }
    }
}
