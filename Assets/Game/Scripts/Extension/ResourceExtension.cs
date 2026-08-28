// 文件职责：提供 资源 相关的通用扩展方法。
// 所属模块：Extension。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;
using GameFramework.Resource;

public static class ResourceExtension
{
    // 加载Asset，并处理完成或失败结果。
    public static void LoadAsset(this ResourceComponent com, string assetName, LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback=null, LoadAssetUpdateCallback loadAssetUpdateCallback = null, LoadAssetDependencyAssetCallback loadAssetDependencyAssetCallback = null)
    {
        GFTrace.Info("Resource", "LoadAsset.Begin", null, GFTrace.Data("asset", assetName));
        LoadAssetCallbacks callbacks = new LoadAssetCallbacks(
            (loadedAssetName, asset, duration, userData) =>
            {
                GFTrace.Success("Resource", "LoadAsset.Success", null, GFTrace.Data("asset", loadedAssetName, "duration", duration.ToString("F3")));
                loadAssetSuccessCallback?.Invoke(loadedAssetName, asset, duration, userData);
            },
            (failedAssetName, status, errorMessage, userData) =>
            {
                GFTrace.Failure("Resource", "LoadAsset.Failure", errorMessage, GFTrace.Data("asset", failedAssetName, "status", status.ToString()));
                loadAssetFailureCallback?.Invoke(failedAssetName, status, errorMessage, userData);
            },
            loadAssetUpdateCallback,
            loadAssetDependencyAssetCallback);
        com.LoadAsset(assetName, callbacks);
    }
}
