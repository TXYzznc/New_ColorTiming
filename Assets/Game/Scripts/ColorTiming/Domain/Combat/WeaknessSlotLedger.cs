// 文件职责：定义 弱点槽位记录器，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTiming.Combat
{
    /// <summary>
    /// Keeps presentation slot indices partitioned by weakness color.
    /// Consuming one color can never read or mutate another color's slots.
    /// </summary>
    public sealed class WeaknessSlotLedger
    {
        private readonly Dictionary<WeaponColor, List<int>> slots;

        // 初始化弱点槽位记录器实例及其核心依赖。
        public WeaknessSlotLedger(IDictionary<WeaponColor, IEnumerable<int>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            slots = source.ToDictionary(
                pair => pair.Key,
                pair => pair.Value?.ToList() ?? throw new ArgumentNullException(nameof(source)));
        }

        // 执行Consume对应的主要流程。
        public int Consume(WeaponColor color)
        {
            if (!slots.TryGetValue(color, out var colorSlots) || colorSlots.Count == 0)
            {
                throw new InvalidOperationException($"No presentation slot remains for {color}.");
            }

            var slot = colorSlots[0];
            colorSlots.RemoveAt(0);
            return slot;
        }

        // 执行剩余数量对应的主要流程。
        public IReadOnlyList<int> Remaining(WeaponColor color)
        {
            return slots.TryGetValue(color, out var colorSlots)
                ? colorSlots
                : Array.Empty<int>();
        }
    }
}
