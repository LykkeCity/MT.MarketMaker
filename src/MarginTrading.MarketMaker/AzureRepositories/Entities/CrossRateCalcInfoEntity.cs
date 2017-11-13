using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.MarketMaker.AzureRepositories.Entities
{
    internal class CrossRateCalcInfoEntity : TableEntity
    {
        public CrossRateCalcInfoEntity()
        {
            PartitionKey = "CrossRateCalcInfo";
        }

        public string ResultingPairId
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string SourcePairId1 { get; set; }
        public bool IsTransitoryAssetQuoting1 { get; set; }
        public string SourcePairId2 { get; set; }
        public bool IsTransitoryAssetQuoting2 { get; set; }
    }
}
