using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal static class Trace
    {
        public static ITraceService TraceService { get; set; }

        public static void Write(TraceGroupEnum group, string assetPairId, string msg, object obj)
        {
            TraceService.Write(group, assetPairId, msg, obj);
        }
    }
}
