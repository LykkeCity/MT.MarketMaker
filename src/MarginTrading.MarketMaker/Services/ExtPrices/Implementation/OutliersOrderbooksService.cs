using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class OutliersOrderbooksService : IOutliersOrderbooksService
    {
        private readonly IBestPricesService _bestPricesService;
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IPrimaryExchangeService _primaryExchangeService;

        public OutliersOrderbooksService(
            IBestPricesService bestPricesService,
            IExtPricesSettingsService extPricesSettingsService,
            IPrimaryExchangeService primaryExchangeService)
        {
            _bestPricesService = bestPricesService;
            _extPricesSettingsService = extPricesSettingsService;
            _primaryExchangeService = primaryExchangeService;
        }

        public IReadOnlyList<ExternalOrderbook> FindOutliers(string assetPairId,
            ImmutableDictionary<string, ExternalOrderbook> validOrderbooks)
        {
            var contexts = validOrderbooks.Values.Select(o =>
            {
                var bestPrices = _bestPricesService.Calc(o);
                return new CalcContext
                {
                    Orderbook = o,
                    Bid = bestPrices.BestBid,
                    Ask = bestPrices.BestAsk,
                };
            }).OrderBy(c => c.Orderbook.ExchangeName).ToList();

            foreach (var context in contexts)
            {
                context.BidRank = contexts.Count(c => c.Bid < context.Bid);
                context.AskRank = contexts.Count(c => c.Ask < context.Ask);
            }

            foreach (var context in contexts)
            {
                context.BidDistancesSum = contexts.Select(c => Square(c.BidRank - context.BidRank)).Sum();
                context.AskDistancesSum = contexts.Select(c => Square(c.AskRank - context.AskRank)).Sum();
                context.DistancesSum = context.BidDistancesSum + context.AskDistancesSum;
            }

            var minDistance = contexts.Min(c => c.DistancesSum);

            CalcContext center;
            var possibleCenters = contexts.Where(c => c.DistancesSum == minDistance).ToList();
            if (possibleCenters.Count == 1)
            {
                center = possibleCenters[0];
            }
            else
            {
                var primaryExchange = _primaryExchangeService.GetLastPrimaryExchange(assetPairId);
                center = possibleCenters.FirstOrDefault(c => c.Orderbook.ExchangeName == primaryExchange);
                if (center == null)
                {
                    var qualities = _primaryExchangeService.GetQualities(assetPairId);
                    center = possibleCenters
                        .Select(c => new { c, quality = qualities.GetValueOrDefault(c.Orderbook.ExchangeName) })
                        .OrderBy(c => c.quality?.ErrorState ?? ExchangeErrorStateDomainEnum.Disabled)
                        .ThenByDescending(c => c.quality.HedgingPreference)
                        .First().c;
                }
            }

            foreach (var context in contexts)
            {
                context.BidRelativeDiff = (context.Bid - center.Bid) / center.Bid;
                context.AskRelativeDiff = (context.Ask - center.Ask) / center.Ask;
            }

            var relativeThreshold = _extPricesSettingsService.GetOutlierThreshold(assetPairId);
            var result = new List<ExternalOrderbook>();
            foreach (var context in contexts)
            {
                if (Math.Abs(context.BidRelativeDiff) >= relativeThreshold)
                {
                    Trace.Write(assetPairId + " err trace", "Outlier (bid)", new
                    {
                        context.Orderbook.AssetPairId,
                        context.Orderbook.ExchangeName,
                        context.Bid,
                        context.BidRank,
                        context.BidDistancesSum,
                        context.DistancesSum,
                        centerBid = center.Bid,
                        centerExchange = center.Orderbook.ExchangeName,
                        context.BidRelativeDiff,
                        relativeThreshold
                    });
                    result.Add(context.Orderbook);
                }
                else if (Math.Abs(context.AskRelativeDiff) >= relativeThreshold)
                {
                    Trace.Write(assetPairId + " err trace", "Outlier (ask)", new
                    {
                        context.Orderbook.AssetPairId,
                        context.Orderbook.ExchangeName,
                        context.Ask,
                        context.AskRank,
                        context.AskDistancesSum,
                        context.DistancesSum,
                        centerAsk = center.Ask,
                        centerExchange = center.Orderbook.ExchangeName,
                        context.AskRelativeDiff,
                        relativeThreshold
                    });
                    result.Add(context.Orderbook);
                }
            }

            return result;
        }

        private static int Square(int d)
        {
            return d * d;
        }

        private class CalcContext
        {
            public ExternalOrderbook Orderbook { get; set; }
            public decimal Bid { get; set; }
            public decimal Ask { get; set; }
            public decimal BidRelativeDiff { get; set; }
            public decimal AskRelativeDiff { get; set; }
            public int BidRank { get; set; }
            public int AskRank { get; set; }
            public int BidDistancesSum { get; set; }
            public int AskDistancesSum { get; set; }
            public int DistancesSum { get; set; }
        }
    }
}