using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class BestPricesService : IBestPricesService
    {
        private readonly ReadWriteLockedDictionary<(string, string), BestPrices> _lastBestPrices =
            new ReadWriteLockedDictionary<(string, string), BestPrices>();

        public BestPrices CalcExternal(ExternalOrderbook orderbook)
        {
            var bestPrices = Calc(orderbook);
            _lastBestPrices[(orderbook.AssetPairId, orderbook.ExchangeName)] = bestPrices;
            return bestPrices;
        }

        [Pure]
        public IReadOnlyDictionary<(string AssetPairId, string Exchange), BestPrices> GetLastCalculated()
        {
            return _lastBestPrices;
        }

        [Pure]
        public BestPrices Calc(Orderbook orderbook)
        {
            return new BestPrices(
                orderbook.Bids.Max(b => b.Price),
                orderbook.Asks.Min(b => b.Price));
        }
    }
}
