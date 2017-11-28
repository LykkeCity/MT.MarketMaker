﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class AssetPairExtPriceSettingsModel
    {
        public string AssetPairId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public MarkupsModel Markups { get; set; }
        public string PresetDefaultExchange { get; set; }
        public decimal OutlierThreshold { get; set; }
        public RepeatedOutliersParamsModel RepeatedOutliers { get; set; }
        public ImmutableDictionary<OrderbookGeneratorStepEnum, bool> Steps { get; set; }
        public TimeSpan MinOrderbooksSendingPeriod { get; set; }
    }
}
