using System;
using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class AggregateOrderbookSettings
    {
        public int AsIsLevelsCount { get; }
        public ImmutableSortedSet<decimal> CumulativeVolumeLevels { get; }
        public decimal RandomFraction { get; }
        

        public AggregateOrderbookSettings(int asIsLevelsCount, ImmutableSortedSet<decimal> cumulativeVolumeLevels,
            decimal randomFraction)
        {
            AsIsLevelsCount = asIsLevelsCount;
            CumulativeVolumeLevels = cumulativeVolumeLevels ?? throw new ArgumentNullException(nameof(cumulativeVolumeLevels));
            RandomFraction = randomFraction;
        }
    }
}