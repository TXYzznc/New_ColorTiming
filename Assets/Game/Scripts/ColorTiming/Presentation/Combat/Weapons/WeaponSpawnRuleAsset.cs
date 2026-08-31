// 文件职责：定义可复用的武器生成规则配置。
// 所属模块：ColorTiming / Presentation / Combat / Weapons。

using System;
using System.Collections.Generic;
using ColorTiming.Combat;
using ColorTiming.Player;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorTiming/Combat/Weapon Spawn Rule", fileName = "WeaponSpawnRule")]
public sealed class WeaponSpawnRuleAsset : ScriptableObject
{
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int activeLimit = 5;
    [SerializeField] private int guaranteeThreshold = 3;
    [SerializeField] private WeaponSpawnEntry[] entries = Array.Empty<WeaponSpawnEntry>();

    public float SpawnInterval => spawnInterval;

    /// <summary>Returns the exact authored combinations that have corresponding presentation resources.</summary>
    public IReadOnlyList<WeaponIdentity> GetSupportedWeapons()
    {
        var weapons = new WeaponIdentity[entries.Length];
        for (int index = 0; index < entries.Length; index++)
        {
            weapons[index] = new WeaponIdentity(entries[index].Color, entries[index].Type);
        }

        return weapons;
    }

    public WeaponSpawnPolicy CreatePolicy()
    {
        return new WeaponSpawnPolicy(GetSupportedWeapons(), activeLimit, guaranteeThreshold);
    }

    [Serializable]
    public struct WeaponSpawnEntry
    {
        [SerializeField] private WeaponColor color;
        [SerializeField] private WeaponType type;

        public WeaponColor Color => color;
        public WeaponType Type => type;
    }
}
