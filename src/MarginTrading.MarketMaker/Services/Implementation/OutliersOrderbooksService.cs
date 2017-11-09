﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Infrastructure.Implemetation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    public class OutliersOrderbooksService : IOutliersOrderbooksService
    {
        private readonly IBestPricesService _bestPricesService;
        private readonly IPriceCalcSettingsService _priceCalcSettingsService;
        private readonly IPrimaryExchangeService _primaryExchangeService;

        public OutliersOrderbooksService(
            IBestPricesService bestPricesService,
            IPriceCalcSettingsService priceCalcSettingsService,
            IPrimaryExchangeService primaryExchangeService)
        {
            _bestPricesService = bestPricesService;
            _priceCalcSettingsService = priceCalcSettingsService;
            _primaryExchangeService = primaryExchangeService;
        }

        public IReadOnlyList<ExternalOrderbook> FindOutliers(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> validOrderbooks)
        {
            var bestPrices = validOrderbooks.Values
                .Select(o => (Orderbook: o, BestPrices: _bestPricesService.CalcExternal(o)))
                .ToList();

            var result = new List<ExternalOrderbook>();
            var medianBid = GetMedian(bestPrices.Select(p => p.BestPrices.BestBid), true);
            var medianAsk = GetMedian(bestPrices.Select(p => p.BestPrices.BestAsk), false);
            var threshold = GetOutlierThreshold(assetPairId);
            foreach (var (orderbook, prices) in bestPrices)
            {
                if (Math.Abs(prices.BestBid - medianBid) > threshold * medianBid)
                {
                    Trace.Write("Outlier (bid)", new
                    {
                        orderbook.AssetPairId,
                        orderbook.ExchangeName,
                        prices.BestBid,
                        medianBid,
                        deviation = Math.Abs(prices.BestBid - medianBid),
                        threshold,
                        thresholdBid = threshold * medianBid
                    });
                    result.Add(orderbook);
                }
                else if (Math.Abs(prices.BestAsk - medianAsk) > threshold * medianAsk)
                {
                    Trace.Write("Outlier (ask)", new
                    {
                        orderbook.AssetPairId,
                        orderbook.ExchangeName,
                        prices.BestAsk,
                        medianAsk,
                        deviation = Math.Abs(prices.BestAsk - medianAsk),
                        threshold,
                        thresholdAsk = threshold * medianAsk
                    });
                    result.Add(orderbook);
                }
            }

            return result;
        }

        private decimal GetOutlierThreshold(string assetPairId)
        {
            return _priceCalcSettingsService.GetOutlierThreshold(assetPairId);
        }

        private static decimal GetMedian(IEnumerable<decimal> src, bool onEvenCountGetLesser)
        {
            // todo: choose using "2d ranked range median" algo
            var sorted = src.OrderBy(e => e).ToList();
            int mid = sorted.Count / 2;
            if (sorted.Count % 2 != 0)
                return sorted[mid];
            else if (onEvenCountGetLesser)
                return sorted[mid - 1];
            else
                return sorted[mid];
        }
    }
}