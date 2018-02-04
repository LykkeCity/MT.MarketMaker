using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class AssetPairSettings
    {
        public AssetPairQuotesSourceTypeDomainEnum QuotesSourceType { get; }
        public AssetPairExtPriceSettings ExtPriceSettings { get; }
        public CrossRateCalcInfo CrossRateCalcInfo { get; }
        public AggregateOrderbookSettings AggregateOrderbookSettings { get; }

        public AssetPairSettings(AssetPairQuotesSourceTypeDomainEnum quotesSourceType,
            AssetPairExtPriceSettings extPriceSettings, CrossRateCalcInfo crossRateCalcInfo,
            AggregateOrderbookSettings aggregateOrderbookSettings)
        {
            QuotesSourceType = quotesSourceType;
            ExtPriceSettings = extPriceSettings;
            CrossRateCalcInfo = crossRateCalcInfo;
            AggregateOrderbookSettings = aggregateOrderbookSettings;
        }
    }
}