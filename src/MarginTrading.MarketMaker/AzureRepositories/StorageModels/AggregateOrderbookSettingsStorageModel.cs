using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AggregateOrderbookSettingsStorageModel
    {
        public int AsIsLevelsCount { get; set; }
        public ImmutableSortedSet<decimal> CumulativeVolumeLevels { get; set; }
        public decimal RandomFraction { get; set; }
    }
}