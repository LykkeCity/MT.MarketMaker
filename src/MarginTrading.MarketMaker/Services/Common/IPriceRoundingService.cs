using System;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IPriceRoundingService
    {
        Func<decimal, decimal> GetRoundFunc(string assetPairId);
    }
}