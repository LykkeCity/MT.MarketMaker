namespace MarginTrading.MarketMaker.Models.Settings
{
    public class AssetPairMarkupsParams
    {
        public decimal Bid { get; }
        public decimal Ask { get; }

        public AssetPairMarkupsParams(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}