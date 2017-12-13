using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal static class Trace
    {
        [CanBeNull]
        public static ITraceService TraceService { get; set; }

        public static void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj)
        {
            TraceService?.Write(levelGroup, assetPairId, msg, obj);
        }
    }
}
