namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class CrossRateCalcInfoStorageModel
    {
        public string ResultingPairId { get; set; }
        
        public CrossRateSourceAssetPair Source1 { get; set; }
            = new CrossRateSourceAssetPair();

        public CrossRateSourceAssetPair Source2 { get; set; }
            = new CrossRateSourceAssetPair();

        public class CrossRateSourceAssetPair
        {
            public string Id { get; set; }
            public bool IsTransitoryAssetQuoting { get; set; }
        }
    }
}