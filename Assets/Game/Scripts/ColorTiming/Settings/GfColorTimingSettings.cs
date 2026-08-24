using System;

namespace ColorTiming.Settings
{
    /// <summary>ColorTiming's typed boundary over GF.Setting and GF.Sound groups.</summary>
    public sealed class GfColorTimingSettings : IColorTimingSettings
    {
        const string KeyTipsDisabledKey = "ColorTiming.UI.KeyTipsDisabled";

        public bool BgmEnabled
        {
            get => !GF.Setting.GetMediaMute(Const.SoundGroup.BGM, false);
            set => SetSoundEnabled(value, Const.SoundGroup.BGM, Const.SoundGroup.Music);
        }

        public bool SfxEnabled
        {
            get => !GF.Setting.GetMediaMute(Const.SoundGroup.Player, false);
            set => SetSoundEnabled(value,
                Const.SoundGroup.UI,
                Const.SoundGroup.Player,
                Const.SoundGroup.Boss,
                Const.SoundGroup.Environment,
                Const.SoundGroup.Sound);
        }

        public bool KeyTipsDisabled
        {
            get => RequireSetting().GetBool(KeyTipsDisabledKey, false);
            set
            {
                var setting = RequireSetting();
                setting.SetBool(KeyTipsDisabledKey, value);
                setting.Save();
            }
        }

        void SetSoundEnabled(bool enabled, params Const.SoundGroup[] groups)
        {
            RequireSetting();
            foreach (var group in groups)
            {
                GF.Setting.SetMediaMute(group, !enabled);
            }
            GFBuiltin.Setting.Save();
        }

        static UnityGameFramework.Runtime.SettingComponent RequireSetting()
        {
            return GFBuiltin.Setting
                ?? throw new InvalidOperationException("GF.Setting is unavailable; ColorTiming must start through Launch.");
        }
    }
}
