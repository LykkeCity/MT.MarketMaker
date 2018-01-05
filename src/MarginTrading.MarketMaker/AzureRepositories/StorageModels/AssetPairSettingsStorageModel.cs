using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AssetPairSettingsStorageModel
    {
        public AssetPairQuotesSourceTypeDomainEnum QuotesSourceType { get; set; }
        [CanBeNull]
        public AssetPairExtPriceSettingsStorageModel ExtPriceSettings { get; set; }
        [CanBeNull]
        public CrossRateCalcInfoStorageModel CrossRateCalcInfo { get; set; }
    }
}