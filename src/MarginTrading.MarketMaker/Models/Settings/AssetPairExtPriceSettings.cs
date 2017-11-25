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
        public ImmutableDictionary<OrderbookGeneratorStepEnum, bool> Steps { get; }
        public ImmutableDictionary<string, ExchangeExtPriceSettings> Exchanges { get; }

        public AssetPairExtPriceSettings(string presetDefaultExchange, decimal outlierThreshold,
            TimeSpan minOrderbooksSendingPeriod, AssetPairMarkupsParams markups, RepeatedOutliersParams repeatedOutliers,
            ImmutableDictionary<OrderbookGeneratorStepEnum, bool> steps,
            ImmutableDictionary<string, ExchangeExtPriceSettings> exchanges)
        {
            PresetDefaultExchange =
                presetDefaultExchange ?? throw new ArgumentNullException(nameof(presetDefaultExchange));
            OutlierThreshold = outlierThreshold;
            MinOrderbooksSendingPeriod = minOrderbooksSendingPeriod;
            Exchanges = exchanges ?? throw new ArgumentNullException(nameof(exchanges));
            Markups = markups ?? throw new ArgumentNullException(nameof(markups));
            RepeatedOutliers = repeatedOutliers ?? throw new ArgumentNullException(nameof(repeatedOutliers));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }
    }
}