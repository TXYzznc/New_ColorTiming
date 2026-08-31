// 文件职责：描述一次战斗资源准备请求，不绑定 Unity 场景名称。
// 所属模块：ColorTiming / Bootstrap。

using System;
using System.Collections.Generic;
using ColorTiming.Combat;

namespace ColorTiming.Bootstrap
{
    /// <summary>
    /// Immutable resource request used for both a scene entry and a new level/wave inside
    /// an already loaded scene. Its content is derived from authoring configuration rather
    /// than a boss-specific code path.
    /// </summary>
    public sealed class BattleLoadContext
    {
        readonly WeaponIdentity[] requiredWeapons;

        public BattleLoadContext(string id, IReadOnlyList<WeaponIdentity> requiredWeapons)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A battle load context requires an id.", nameof(id));
            Id = id;

            var uniqueWeapons = new List<WeaponIdentity>();
            if (requiredWeapons != null)
            {
                for (var i = 0; i < requiredWeapons.Count; i++)
                {
                    if (!uniqueWeapons.Contains(requiredWeapons[i])) uniqueWeapons.Add(requiredWeapons[i]);
                }
            }
            this.requiredWeapons = uniqueWeapons.ToArray();
        }

        public string Id { get; }
        public IReadOnlyList<WeaponIdentity> RequiredWeapons => requiredWeapons;
    }
}
