using System;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IRepeatedProblemsOrderbooksService
    {
        bool IsRepeatedProblemsOrderbook(ExternalOrderbook orderbook, bool isOutdated, bool isOutlier, DateTime now);
    }
}
