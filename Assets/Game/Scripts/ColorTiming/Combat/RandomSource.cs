using System;

namespace ColorTiming.Combat
{
    public interface IRandomSource
    {
        int Range(int minimumInclusive, int maximumExclusive);
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        private readonly Random random;

        public SeededRandomSource(int seed)
        {
            random = new Random(seed);
        }

        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return random.Next(minimumInclusive, maximumExclusive);
        }
    }
}
