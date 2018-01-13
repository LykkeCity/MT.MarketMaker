using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class AssetPairExtPriceSettings
    {
        public string PresetDefaultExchange { get; }
        public decimal OutlierThreshold { get; }
        public TimeSpan MinOrderbooksSendingPeriod { get; }
        public AssetPairMarkupsParams Markups { get; }
        public RepeatedOutliersParams RepeatedOutliers { get; }
        public ImmutableDictionary<OrderbookGeneratorStepDomainEnum, bool> Steps { get; }
        public ImmutableDictionary<string, ExchangeExtPriceSettings> Exchanges { get; }

        public AssetPairExtPriceSettings(string presetDefaultExchange, decimal outlierThreshold,
            TimeSpan minOrderbooksSendingPeriod, AssetPairMarkupsParams markups,
            RepeatedOutliersParams repeatedOutliers,
            ImmutableDictionary<OrderbookGeneratorStepDomainEnum, bool> steps,
            ImmutableDictionary<string, ExchangeExtPriceSettings> exchanges)
        {
            PresetDefaultExchange = presetDefaultExchange;
            OutlierThreshold = outlierThreshold;
            MinOrderbooksSendingPeriod = minOrderbooksSendingPeriod;
            Exchanges = exchanges;
            Markups = markups;
            RepeatedOutliers = repeatedOutliers;
            Steps = steps;
        }

        public static AssetPairExtPriceSettings Change(AssetPairExtPriceSettings src,
            ImmutableDictionary<string, ExchangeExtPriceSettings> exchanges)
        {
            return new AssetPairExtPriceSettings(src.PresetDefaultExchange, src.OutlierThreshold,
                src.MinOrderbooksSendingPeriod, src.Markups, src.RepeatedOutliers, src.Steps, exchanges);
        }
    }
}