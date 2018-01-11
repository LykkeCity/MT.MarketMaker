using System;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IPriceRoundingService
    {
        decimal Round(string assetPairId, decimal price);
        Func<decimal, decimal> GetRoundFunc(string assetPairId);
    }
}