using System.Collections.Immutable;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IOrderbooksService
    {
        ImmutableDictionary<string, ExternalOrderbook> AddAndGetByAssetPair(ExternalOrderbook orderbook);
    }
}