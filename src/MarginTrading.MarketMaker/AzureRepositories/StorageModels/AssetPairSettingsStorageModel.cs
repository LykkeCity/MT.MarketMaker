using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AssetPairSettingsStorageModel
    {
        public AssetPairQuotesSourceTypeEnum QuotesSourceType { get; set; }
        [CanBeNull]
        public AssetPairExtPriceSettingsStorageModel ExtPriceSettings { get; set; }
        [CanBeNull]
        public CrossRateCalcInfoStorageModel CrossRateCalcInfo { get; set; }
    }
}