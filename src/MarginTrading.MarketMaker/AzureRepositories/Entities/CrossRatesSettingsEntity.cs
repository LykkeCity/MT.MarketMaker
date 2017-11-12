using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.MarketMaker.AzureRepositories.Entities
{
    internal class CrossRatesSettingsEntity : TableEntity
    {
        public CrossRatesSettingsEntity()
        {
            PartitionKey = "CrossRatesSettings";
        }

        public string BaseAssetId
        {
            get => RowKey;
            set => RowKey = value;
        }

        [CanBeNull]
        public string OtherAssetsIds { get; set; }
    }
}