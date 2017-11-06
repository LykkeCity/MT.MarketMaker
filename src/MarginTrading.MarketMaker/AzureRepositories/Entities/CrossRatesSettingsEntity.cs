using JetBrains.Annotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.MarketMaker.AzureRepositories.Entities
{
    internal class CrossRatesSettingsEntity : TableEntity
    {
        public CrossRatesSettingsEntity()
        {
            PartitionKey = "CrossRatesSettings";
            RowKey = "CrossRatesSettings";
        }

        [CanBeNull]
        public string BaseAssetsIds { get; set; }
        
        [CanBeNull]
        public string OtherAssetsIds { get; set; }
    }
}