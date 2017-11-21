using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AssetPairExtPriceSettingsStorageModel
    {
        public string PresetDefaultExchange { get; set; }
        public decimal OutlierThreshold { get; set; }
        public TimeSpan MinOrderbooksSendingPeriod { get; set; }
        public MarkupsParams Markups { get; set; } = new MarkupsParams();
        public RepeatedOutliersParams RepeatedOutliers { get; set; } = new RepeatedOutliersParams();

        public ImmutableDictionary<OrderbookGeneratorStepEnum, bool> Steps { get; set; }
            = ImmutableDictionary<OrderbookGeneratorStepEnum, bool>.Empty;

        public class MarkupsParams
        {
            public decimal Bid { get; set; }
            public decimal Ask { get; set; }
        }

        public class RepeatedOutliersParams
        {
            public int MaxSequenceLength { get; set; }
            public TimeSpan MaxSequenceAge { get; set; }
            public decimal MaxAvg { get; set; }
            public TimeSpan MaxAvgAge { get; set; }
        }
    }
}