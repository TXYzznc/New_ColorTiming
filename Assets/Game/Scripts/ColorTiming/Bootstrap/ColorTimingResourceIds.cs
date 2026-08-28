// 文件职责：定义 ColorTiming资源ID，承担 Bootstrap 模块中的对应职责。
// 所属模块：ColorTiming / Bootstrap。

namespace ColorTiming.Bootstrap
{
    /// <summary>
    /// Canonical product identifiers passed to the framework path helpers.
    /// </summary>
    public static class ColorTimingResourceIds
    {
        public const string Product = "ColorTiming";

        public const string StartMenuScene = "StartMenu";
        public const string Boss1Scene = "Boss1";
        public const string Boss2Scene = "Boss2";

        public const string EntityPrefix = Product + "/";
        public const string UiPrefix = Product + "/";
        public const string SoundPrefix = Product + "/";

        public const string SceneResource = "Scene";
        public const string EntityResource = "Entity";
        public const string UiResource = "UI";
        public const string WorldResource = "World";
        public const string SoundResource = "Sound";
        public const string ConfigResource = "Config";
        public const string DataTableResource = "DataTable";

        // 执行实体对应的主要流程。
        public static string Entity(string relativeName) => EntityPrefix + relativeName;
        // 执行UI对应的主要流程。
        public static string Ui(string relativeName) => UiPrefix + relativeName;
        // 执行音效对应的主要流程。
        public static string Sound(string relativeName) => SoundPrefix + relativeName;
    }
}
