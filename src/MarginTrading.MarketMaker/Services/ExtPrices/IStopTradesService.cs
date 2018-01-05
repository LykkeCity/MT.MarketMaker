using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IStopTradesService
    {
        void SetPrimaryOrderbookState(string assetPairId, string exchange, DateTime now, decimal hedgingPreference, ExchangeErrorStateDomainEnum? errorState);

        void SetFreshOrderbooksState(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> freshOrderbooks, DateTime now);

        void FinishCycle(ExternalOrderbook primaryOrderbook, DateTime now);
    }
}