using System.Diagnostics;

namespace MarginTrading.MarketMaker.Models
{
    [DebuggerDisplay("{Volume} for {Price}")]
    public struct OrderbookPosition
    {
        public OrderbookPosition(decimal price, decimal volume)
        {
            Volume = volume;
            Price = price;
        }

        public decimal Price { get; }
        public decimal Volume { get; }

        public bool Equals(OrderbookPosition other)
        {
            return Price == other.Price && Volume == other.Volume;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OrderbookPosition position && Equals(position);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Price.GetHashCode() * 397) ^ Volume.GetHashCode();
            }
        }

        public static bool operator ==(OrderbookPosition first, OrderbookPosition second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(OrderbookPosition first, OrderbookPosition second)
        {
            return !(first == second);
        }
    }
}
