namespace MarginTrading.MarketMaker.Models.Api
{
    public class CrossRateCalcInfoModel
    {
        public string ResultingPairId { get; set; }
        public CrossRateSourceAssetPairModel Source1 { get; set; }
        public CrossRateSourceAssetPairModel Source2 { get; set; }
    }
}