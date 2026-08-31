// 文件职责：保存 Hero 武器动画候选 Controller 的资源标识映射。
// 所属模块：ColorTiming / Presentation / Actors / Player。

using System;
using ColorTiming.Combat;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorTiming/Player/Hero Weapon Animation Catalog", fileName = "HeroWeaponAnimationCatalog")]
public sealed class HeroWeaponAnimationCatalogAsset : ScriptableObject
{
    [SerializeField] private string baseControllerAssetName;
    [SerializeField] private Entry[] entries = Array.Empty<Entry>();

    public bool TryGetControllerAssetName(WeaponIdentity weapon, out string assetName)
    {
        if (weapon.IsNormal)
        {
            assetName = baseControllerAssetName;
            return !string.IsNullOrEmpty(assetName);
        }

        for (int index = 0; index < entries.Length; index++)
        {
            if (entries[index].Color != weapon.Color || entries[index].Type != weapon.Type) continue;
            assetName = entries[index].ControllerAssetName;
            return !string.IsNullOrEmpty(assetName);
        }

        assetName = null;
        return false;
    }

    [Serializable]
    public struct Entry
    {
        [SerializeField] private WeaponColor color;
        [SerializeField] private WeaponType type;
        [SerializeField] private string controllerAssetName;

        public WeaponColor Color => color;
        public WeaponType Type => type;
        public string ControllerAssetName => controllerAssetName;
    }
}
