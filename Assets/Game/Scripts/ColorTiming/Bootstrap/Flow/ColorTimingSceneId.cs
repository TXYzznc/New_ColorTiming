// 文件职责：定义 ColorTiming场景ID，承担 流程 模块中的对应职责。
// 所属模块：ColorTiming / Bootstrap / Flow。

using System;

namespace ColorTiming.Bootstrap.Flow
{
    public enum ColorTimingSceneId
    {
        StartMenu = 0,
        Boss1 = 1,
        Boss2 = 2,
    }

    public static class ColorTimingSceneIdExtensions
    {
        // 执行To资源Name对应的主要流程。
        public static string ToResourceName(this ColorTimingSceneId scene)
        {
            switch (scene)
            {
                case ColorTimingSceneId.StartMenu:
                    return "StartMenu";
                case ColorTimingSceneId.Boss1:
                    return "Boss1";
                case ColorTimingSceneId.Boss2:
                    return "Boss2";
                default:
                    throw new ArgumentOutOfRangeException(nameof(scene), scene, "Unknown ColorTiming scene.");
            }
        }
    }
}
