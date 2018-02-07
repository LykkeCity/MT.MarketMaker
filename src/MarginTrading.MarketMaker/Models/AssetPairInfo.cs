namespace MarginTrading.MarketMaker.Models
{
    public class AssetPairInfo
    {
        public string Id { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
        public string Source { get; set; }
        public string Source2 { get; set; }
    }
}