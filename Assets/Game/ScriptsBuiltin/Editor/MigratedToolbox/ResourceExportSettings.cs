using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity Package 资源导出工具的资源组配置。
/// </summary>
[CreateAssetMenu(
    fileName = "ResourceExportSettings",
    menuName = "Game Framework/Editor Tools/Resource Export Settings"
)]
public sealed class ResourceExportSettings : ScriptableObject
{
    public List<ResourceGroup> groups = new();

    [Serializable]
    public sealed class ResourceGroup
    {
        public string name = "新资源组";
        public bool selected = true;
        public List<string> assetPaths = new();
    }
}
