// 文件职责：定义 ColorTiming设置 的依赖契约，供模块间解耦使用。
// 所属模块：ColorTiming / Settings。

namespace ColorTiming.Settings
{
    public interface IColorTimingSettings
    {
        bool BgmEnabled { get; set; }
        bool SfxEnabled { get; set; }
        bool KeyTipsDisabled { get; set; }
    }

    public interface IColorTimingSettingsConsumer
    {
        // 绑定设置依赖或事件监听。
        void BindSettings(IColorTimingSettings settings);
    }
}
