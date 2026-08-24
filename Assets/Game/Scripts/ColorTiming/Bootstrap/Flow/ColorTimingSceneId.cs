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
