using System;

namespace MarginTrading.MarketMaker.Infrastructure.Implemetation
{
    internal class SystemService : ISystem
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
