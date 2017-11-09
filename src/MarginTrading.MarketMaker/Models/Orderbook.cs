using System.Collections;
using System.Collections.Generic;
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
            AssetPairId = assetPairId;
            Bids = bids;
            Asks = asks;
            AssetPairId = assetPairId;
        }

        private sealed class EqualityComparer : IEqualityComparer<Orderbook>
        {
            public bool Equals(Orderbook x, Orderbook y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                var comparer = StructuralComparisons.StructuralEqualityComparer;
                return comparer.Equals(x.Bids, y.Bids) && comparer.Equals(x.Asks, y.Asks) && string.Equals(x.AssetPairId, y.AssetPairId);
            }

            public int GetHashCode(Orderbook obj)
            {
                var comparer = StructuralComparisons.StructuralEqualityComparer;
                unchecked
                {
                    var hashCode = comparer.GetHashCode(obj.Bids);
                    hashCode = (hashCode * 397) ^ comparer.GetHashCode(obj.Asks);
                    hashCode = (hashCode * 397) ^ (obj.AssetPairId != null ? obj.AssetPairId.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<Orderbook> Comparer { get; } = new EqualityComparer();
    }
}