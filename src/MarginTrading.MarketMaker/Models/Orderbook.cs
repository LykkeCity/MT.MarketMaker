using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models
{
    public class Orderbook
    {
        public ImmutableArray<OrderbookPosition> Bids { get; }
        public ImmutableArray<OrderbookPosition> Asks { get; }
        public string AssetPairId { get; }

        public Orderbook(string assetPairId, ImmutableArray<OrderbookPosition> bids, ImmutableArray<OrderbookPosition> asks)
        {
            Bids = bids;
            Asks = asks;
            AssetPairId = assetPairId;
        }
    }
}