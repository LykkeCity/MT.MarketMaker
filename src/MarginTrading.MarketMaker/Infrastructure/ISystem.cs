using System;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface ISystem
    {
        DateTime UtcNow { get; }
    }
}