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
        void BindSettings(IColorTimingSettings settings);
    }
}
