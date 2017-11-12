using System;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal class SystemService : ISystem
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
