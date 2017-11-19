using System;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class AssetPairSettingsModel
    {
        public string AssetPairId { get; set; }
        public AssetPairQuotesSourceTypeEnum? QuotesSourceType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public decimal? ManualBid { get; set; }
        public decimal? ManualAsk { get; set; }
    }
}