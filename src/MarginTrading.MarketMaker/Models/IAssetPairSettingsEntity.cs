﻿using System;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models
{
    [Obsolete]
    internal interface IAssetPairSettingsEntity
    {
        string AssetPairId { get; set; }
        AssetPairQuotesSourceTypeEnum QuotesSourceType { get; set; }
        DateTimeOffset Timestamp { get; set; }
    }
}
