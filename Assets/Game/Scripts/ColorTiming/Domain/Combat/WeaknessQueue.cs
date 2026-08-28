// 文件职责：定义 弱点队列，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTiming.Combat
{
    public sealed class WeaknessQueue
    {
        private readonly List<WeaponColor> segments;

        private WeaknessQueue(IEnumerable<WeaponColor> source, IRandomSource random)
        {
            segments = new List<WeaponColor>(source);
            for (var index = segments.Count - 1; index > 0; index--)
            {
                var other = random.Range(0, index + 1);
                var value = segments[index];
                segments[index] = segments[other];
                segments[other] = value;
            }
        }

        public int Count => segments.Count;
        public bool IsEmpty => segments.Count == 0;
        public WeaponColor Current => !IsEmpty
            ? segments[0]
            : throw new InvalidOperationException("The weakness queue is empty.");

        // 创建Boss1并完成必要的初始配置。
        public static WeaknessQueue CreateBoss1(IRandomSource random)
        {
            return Create(random, 4, 3, 4, 0);
        }

        // 创建Boss2并完成必要的初始配置。
        public static WeaknessQueue CreateBoss2(IRandomSource random)
        {
            return Create(random, 4, 4, 4, 3);
        }

        // 执行Upcoming对应的主要流程。
        public IReadOnlyList<WeaponColor> Upcoming(int maximum = 7)
        {
            if (maximum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum));
            }

            return segments.Take(maximum).ToArray();
        }

        // 移除当前项并清理相关引用。
        public WeaponColor RemoveCurrent()
        {
            var removed = Current;
            segments.RemoveAt(0);
            return removed;
        }

        // 执行数量对应的主要流程。
        public int CountOf(WeaponColor color) => segments.Count(value => value == color);

        // 创建并初始化新的实例。
        private static WeaknessQueue Create(
            IRandomSource random,
            int red,
            int green,
            int purple,
            int orange)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var values = new List<WeaponColor>(red + green + purple + orange);
            Add(values, WeaponColor.Red, red);
            Add(values, WeaponColor.Green, green);
            Add(values, WeaponColor.Purple, purple);
            Add(values, WeaponColor.Orange, orange);
            return new WeaknessQueue(values, random);
        }

        private static void Add(ICollection<WeaponColor> values, WeaponColor color, int count)
        {
            for (var index = 0; index < count; index++)
            {
                values.Add(color);
            }
        }
    }
}
