using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Models.Settings
{
    internal class AssetPairSettings
    {
        public AssetPairQuotesSourceTypeEnum QuotesSourceType { get; }

        [CanBeNull]
        public AssetPairExtPriceSettings ExtPriceSettings { get; }

        [CanBeNull]
        public CrossRateCalcInfo CrossRateCalcInfo { get; }

        public AssetPairSettings(AssetPairQuotesSourceTypeEnum quotesSourceType, [CanBeNull] AssetPairExtPriceSettings extPriceSettings, [CanBeNull] CrossRateCalcInfo crossRateCalcInfo)
        {
            QuotesSourceType = quotesSourceType;
            ExtPriceSettings = extPriceSettings;
            CrossRateCalcInfo = crossRateCalcInfo;
        }
    }
}