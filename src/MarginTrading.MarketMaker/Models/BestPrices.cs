namespace MarginTrading.MarketMaker.Models
{
    public struct BestPrices
    {
        public decimal BestBid { get; }
        public decimal BestAsk { get; }

        public BestPrices(decimal bestBid, decimal bestAsk)
        {
            BestBid = bestBid;
            BestAsk = bestAsk;
        }

        public bool Equals(BestPrices other)
        {
            return BestBid == other.BestBid && BestAsk == other.BestAsk;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BestPrices prices && Equals(prices);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BestBid.GetHashCode() * 397) ^ BestAsk.GetHashCode();
            }
        }
    }
}