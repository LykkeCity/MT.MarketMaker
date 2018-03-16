using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class AssetPairSettingsStorageModel
    {
        public AssetPairQuotesSourceTypeDomainEnum QuotesSourceType { get; set; }
        public AssetPairExtPriceSettingsStorageModel ExtPriceSettings { get; set; }
        public AggregateOrderbookSettingsStorageModel AggregateOrderbookSettings { get; set; }
        public CrossRateCalcInfoStorageModel CrossRateCalcInfo { get; set; }
    }
}