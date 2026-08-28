// 文件职责：提供 本地化 相关的通用扩展方法。
// 所属模块：Extension。

using GameFramework;
using UnityGameFramework.Runtime;

public static class LocalizationExtension
{
    // 加载语言，并处理完成或失败结果。
    public static void LoadLanguage(this LocalizationComponent com, string name, string abTestGroup, bool useBytes, object userData)
    {
        string assetName = name;
        if (!string.IsNullOrWhiteSpace(abTestGroup))
        {
            var abTestAssetName = Utility.Text.Format("{0}{1}{2}", name, ConstBuiltin.AB_TEST_TAG, abTestGroup);
            if (GF.Resource.HasAsset(UtilityBuiltin.AssetsPath.GetLanguagePath(abTestAssetName, useBytes)) != GameFramework.Resource.HasAssetResult.NotExist)
            {
                assetName = abTestAssetName;
            }
        }
        com.ReadData(UtilityBuiltin.AssetsPath.GetLanguagePath(assetName, useBytes), userData);
    }
    // 加载语言，并处理完成或失败结果。
    public static void LoadLanguage(this LocalizationComponent com, string name, bool useBytes, object userData)
    {
        string abTestGroup = GFBuiltin.Setting.GetABTestGroup();
        com.LoadLanguage(name, abTestGroup, useBytes, userData);
    }
    // 加载语言，并处理完成或失败结果。
    public static async void LoadLanguage(this LocalizationComponent com, string name, object userData)
    {
        string abTestGroup = GFBuiltin.Setting.GetABTestGroup();
        var appConfig = await AppConfigs.GetInstanceSync();
        com.LoadLanguage(name, abTestGroup, appConfig.LoadFromBytes, userData);
    }
}
