using System;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IOutdatedOrderbooksService
    {
        bool IsOutdated(ExternalOrderbook orderbook, DateTime now);
    }
}
