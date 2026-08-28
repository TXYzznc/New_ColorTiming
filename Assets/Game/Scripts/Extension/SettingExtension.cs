// 文件职责：提供 设置 相关的通用扩展方法。
// 所属模块：Extension。

using GameFramework;
using UnityGameFramework.Runtime;

/// <summary>
/// Persistent framework settings for localization and generic sound groups.
/// </summary>
public static class SettingExtension
{
    // 设置ABTest分组，并使后续流程使用最新状态。
    public static void SetABTestGroup(this SettingComponent component, string groupName)
    {
        component.SetString(ConstBuiltin.Setting.ABTestGroup, groupName ?? string.Empty);
    }

    // 获取ABTest分组。
    public static string GetABTestGroup(this SettingComponent component)
    {
        return component.GetString(ConstBuiltin.Setting.ABTestGroup, string.Empty);
    }

    // 设置语言，并使后续流程使用最新状态。
    public static void SetLanguage(this SettingComponent component, GameFramework.Localization.Language language, bool saveSetting = true)
    {
        GFBuiltin.Localization.Language = language;
        component.SetString(ConstBuiltin.Setting.Language, language.ToString());
    }

    // 获取语言。
    public static GameFramework.Localization.Language GetLanguage(this SettingComponent component)
    {
        string value = component.GetString(ConstBuiltin.Setting.Language, string.Empty);
        return System.Enum.TryParse(value, out GameFramework.Localization.Language language)
            ? language
            : GameFramework.Localization.Language.Unspecified;
    }

    // 设置MediaMute，并使后续流程使用最新状态。
    public static void SetMediaMute(this SettingComponent component, Const.SoundGroup group, bool isMuted)
    {
        string groupName = group.ToString();
        var soundGroup = GF.Sound.GetSoundGroup(groupName);
        if (soundGroup == null)
        {
            return;
        }

        soundGroup.Mute = isMuted;
        component.SetBool($"Sound.{groupName}.Mute", isMuted);
    }

    // 获取MediaMute。
    public static bool GetMediaMute(this SettingComponent component, Const.SoundGroup group, bool defaultValue = true)
    {
        return component.GetBool($"Sound.{group}.Mute", defaultValue);
    }

    // 设置MediaVolume，并使后续流程使用最新状态。
    public static void SetMediaVolume(this SettingComponent component, Const.SoundGroup group, float volume)
    {
        string groupName = group.ToString();
        var soundGroup = GF.Sound.GetSoundGroup(groupName);
        if (soundGroup == null)
        {
            return;
        }

        soundGroup.Volume = volume;
        component.SetFloat($"Sound.{groupName}.Volume", volume);
    }

    // 获取MediaVolume。
    public static float GetMediaVolume(this SettingComponent component, Const.SoundGroup group, float defaultValue = 1f)
    {
        return component.GetFloat($"Sound.{group}.Volume", defaultValue);
    }
}
