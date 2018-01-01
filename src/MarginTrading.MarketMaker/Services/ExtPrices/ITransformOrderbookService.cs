using System.Collections.Generic;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface ITransformOrderbookService
    {
        Orderbook Transform(ExternalOrderbook primaryOrderbook, IReadOnlyDictionary<string, BestPrices> bestPrices);
    }
}
