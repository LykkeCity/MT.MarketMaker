using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Models.Settings
{
    internal class AssetPairSettings
    {
        public AssetPairQuotesSourceTypeEnum QuotesSourceType { get; set; }

        [CanBeNull]
        public AssetPairExtPriceSettings ExtPriceSettings { get; set; }

        [CanBeNull]
        public CrossRateCalcInfo CrossRateCalcInfo { get; set; }

        public AssetPairSettings(AssetPairQuotesSourceTypeEnum quotesSourceType, [CanBeNull] AssetPairExtPriceSettings extPriceSettings, [CanBeNull] CrossRateCalcInfo crossRateCalcInfo)
        {
            QuotesSourceType = quotesSourceType;
            ExtPriceSettings = extPriceSettings;
            CrossRateCalcInfo = crossRateCalcInfo;
        }
    }
}