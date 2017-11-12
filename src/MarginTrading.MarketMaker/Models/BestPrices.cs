namespace MarginTrading.MarketMaker.Models
{
    public class BestPrices
    {
        public decimal BestBid { get; }
        public decimal BestAsk { get; }

        public BestPrices(decimal bestBid, decimal bestAsk)
        {
            BestBid = bestBid;
            BestAsk = bestAsk;
        }
    }
}