using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AssetPairExtPriceSettingsStorageModel
    {
        public string PresetDefaultExchange { get; set; }
        public decimal OutlierThreshold { get; set; }
        public TimeSpan MinOrderbooksSendingPeriod { get; set; }
        public MarkupsParamsStorageModel Markups { get; set; } = new MarkupsParamsStorageModel();
        public RepeatedOutliersParamsStorageModel RepeatedOutliers { get; set; } = new RepeatedOutliersParamsStorageModel();

        public ImmutableSortedDictionary<OrderbookGeneratorStepDomainEnum, bool> Steps { get; set; }
            = ImmutableSortedDictionary<OrderbookGeneratorStepDomainEnum, bool>.Empty;

        public ImmutableSortedDictionary<string, ExchangeExtPriceSettingsStorageModel> Exchanges { get; set; }
            = ImmutableSortedDictionary<string, ExchangeExtPriceSettingsStorageModel>.Empty;

        public class MarkupsParamsStorageModel
        {
            public decimal Bid { get; set; }
            public decimal Ask { get; set; }
        }

        public class RepeatedOutliersParamsStorageModel
        {
            public int MaxSequenceLength { get; set; }
            public TimeSpan MaxSequenceAge { get; set; }
            public decimal MaxAvg { get; set; }
            public TimeSpan MaxAvgAge { get; set; }
        }
    }
}