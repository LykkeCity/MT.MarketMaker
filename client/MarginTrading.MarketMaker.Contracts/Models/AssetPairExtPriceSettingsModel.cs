using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Contracts.Enums;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class AssetPairExtPriceSettingsModel
    {
        public string AssetPairId { get; set; }
        public string PresetDefaultExchange { get; set; }
        public decimal OutlierThreshold { get; set; }
        public TimeSpan MinOrderbooksSendingPeriod { get; set; }
        public AssetPairMarkupsParamsModel Markups { get; set; }
        public RepeatedOutliersParamsModel RepeatedOutliers { get; set; }
        public ImmutableDictionary<OrderbookGeneratorStepEnum, bool> Steps { get; set; }
    }
}