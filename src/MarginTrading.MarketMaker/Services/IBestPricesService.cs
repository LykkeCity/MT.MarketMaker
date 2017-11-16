using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services
{
    public interface IBestPricesService
    {
        BestPrices CalcExternal(ExternalOrderbook orderbook);
        
        [Pure]
        IReadOnlyDictionary<(string AssetPairId, string Exchange), BestPrices> GetLastCalculated();

        [Pure]
        BestPrices Calc(Orderbook orderbook);
    }
}