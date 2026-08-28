// 文件职责：定义 随机源，承担 Combat 模块中的对应职责。
// 所属模块：ColorTiming / Domain / Combat。

using System;

namespace ColorTiming.Combat
{
    public interface IRandomSource
    {
        // 执行Range对应的主要流程。
        int Range(int minimumInclusive, int maximumExclusive);
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random random;

        // 初始化Seeded随机源Source实例及其核心依赖。
        public SeededRandomSource(int seed)
        {
            random = new Random(seed);
        }

        // 执行Range对应的主要流程。
        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return random.Next(minimumInclusive, maximumExclusive);
        }
    }
}
