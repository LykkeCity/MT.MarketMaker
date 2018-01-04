using System.Collections.Generic;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface ITraceService
    {
        void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj);
        List<TraceModel> GetLast();
        List<TraceModel> GetLast(string contains);
    }
}