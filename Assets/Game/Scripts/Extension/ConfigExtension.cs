// 文件职责：提供 配置 相关的通用扩展方法。
// 所属模块：Extension。

using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
public static class ConfigExtension
{
    // 加载配置，并处理完成或失败结果。
    public static void LoadConfig(this ConfigComponent cfg, string name, string abTestGroup, bool useBytes, object userData)
    {
        string cfgName = name;
        if (!string.IsNullOrWhiteSpace(abTestGroup))
        {
            var abTestCfgName = Utility.Text.Format("{0}{1}{2}", name, ConstBuiltin.AB_TEST_TAG, abTestGroup);
            if (GF.Resource.HasAsset(UtilityBuiltin.AssetsPath.GetConfigPath(abTestCfgName, useBytes)) != GameFramework.Resource.HasAssetResult.NotExist)
            {
                cfgName = abTestCfgName;
            }
        }
        cfg.ReadData(UtilityBuiltin.AssetsPath.GetConfigPath(cfgName, useBytes), userData);
    }
    // 加载配置，并处理完成或失败结果。
    public static void LoadConfig(this ConfigComponent cfg, string name, bool useBytes, object userData)
    {
        string abTestGroup = GFBuiltin.Setting.GetABTestGroup();
        cfg.LoadConfig(name, abTestGroup, useBytes, userData);
    }
    // 获取Vector2Int。
    public static Vector2Int GetVector2Int(this ConfigComponent cfg, string key)
    {
        return cfg.GetVector2Int(key, Vector2Int.zero);
    }
    // 获取Vector2Int。
    public static Vector2Int GetVector2Int(this ConfigComponent cfg, string key, Vector2Int defaultValue = default)
    {
        if (!cfg.HasConfig(key)) return defaultValue;

        return DataTableExtension.ParseVector2Int(cfg.GetString(key));
    }
    // 获取Vector2。
    public static Vector2 GetVector2(this ConfigComponent cfg, string key)
    {
        return cfg.GetVector2(key, Vector2.zero);
    }
    // 获取Vector2。
    public static Vector2 GetVector2(this ConfigComponent cfg, string key, Vector2 defaultValue = default)
    {
        if (!cfg.HasConfig(key)) return defaultValue;

        return DataTableExtension.ParseVector2(cfg.GetString(key));
    }

    // 获取Vector3。
    public static Vector3 GetVector3(this ConfigComponent cfg, string key)
    {
        return cfg.GetVector3(key, Vector3.zero);
    }
    // 获取Vector3。
    public static Vector3 GetVector3(this ConfigComponent cfg, string key, Vector3 defaultValue = default)
    {
        if (!cfg.HasConfig(key)) return defaultValue;

        return DataTableExtension.ParseVector3(cfg.GetString(key));
    }
    public static T[] GetArray<T>(this ConfigComponent cfg, string key, T[] defaultValue = null)
    {
        if (!cfg.HasConfig(key)) return defaultValue;

        return DataTableExtension.ParseArray<T>(cfg.GetString(key));
    }
    // 获取Vector2Array。
    public static Vector2[] GetVector2Array(this ConfigComponent cfg, string key, Vector2[] defaultArr = null)
    {
        if (!cfg.HasConfig(key)) return defaultArr;

        return DataTableExtension.ParseVector2Array(cfg.GetString(key));
    }
    // 获取Vector2IntArray。
    public static Vector2Int[] GetVector2IntArray(this ConfigComponent cfg, string key, Vector2Int[] defaultArr = null)
    {
        if (!cfg.HasConfig(key)) return defaultArr;

        return DataTableExtension.ParseVector2IntArray(cfg.GetString(key));
    }
}
