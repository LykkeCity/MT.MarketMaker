using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class AssetPairExtPriceSettings
    {
        public string PresetDefaultExchange { get; }
        public double OutlierThreshold { get; }
        public TimeSpan MinOrderbooksSendingPeriod { get; }
        public MarkupsParams Markups { get; }
        public RepeatedOutliersParams RepeatedOutliers { get; }
        public ImmutableDictionary<OrderbookGeneratorStepEnum, bool> Steps { get; }

        public AssetPairExtPriceSettings(string presetDefaultExchange, double outlierThreshold,
            TimeSpan minOrderbooksSendingPeriod, MarkupsParams markups, RepeatedOutliersParams repeatedOutliers,
            ImmutableDictionary<OrderbookGeneratorStepEnum, bool> steps)
        {
            PresetDefaultExchange =
                presetDefaultExchange ?? throw new ArgumentNullException(nameof(presetDefaultExchange));
            OutlierThreshold = outlierThreshold;
            MinOrderbooksSendingPeriod = minOrderbooksSendingPeriod;
            Markups = markups ?? throw new ArgumentNullException(nameof(markups));
            RepeatedOutliers = repeatedOutliers ?? throw new ArgumentNullException(nameof(repeatedOutliers));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public class MarkupsParams
        {
            public double Bid { get; }
            public double Ask { get; }

            public MarkupsParams(double bid, double ask)
            {
                Bid = bid;
                Ask = ask;
            }
        }

        public class RepeatedOutliersParams
        {
            public int MaxSequenceLength { get; }
            public TimeSpan MaxSequenceAge { get; }
            public double MaxAvg { get; }
            public TimeSpan MaxAvgAge { get; }

            public RepeatedOutliersParams(int maxSequenceLength, TimeSpan maxSequenceAge, double maxAvg,
                TimeSpan maxAvgAge)
            {
                MaxSequenceLength = maxSequenceLength;
                MaxSequenceAge = maxSequenceAge;
                MaxAvg = maxAvg;
                MaxAvgAge = maxAvgAge;
            }
        }
    }
}