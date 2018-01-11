using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.Common;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class TransformOrderbookService : ITransformOrderbookService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IPriceRoundingService _priceRoundingService;

        public TransformOrderbookService(IExtPricesSettingsService extPricesSettingsService,
            IPriceRoundingService priceRoundingService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _priceRoundingService = priceRoundingService;
        }

        public Orderbook Transform(ExternalOrderbook primaryOrderbook,
            IReadOnlyDictionary<string, BestPrices> bestPrices)
        {
            var isArbitrageFreeSpreadEnabled = _extPricesSettingsService.IsStepEnabled(
                OrderbookGeneratorStepDomainEnum.GetArbitrageFreeSpread,
                primaryOrderbook.AssetPairId);

            var arbitrageFreeSpread = isArbitrageFreeSpreadEnabled
                ? GetArbitrageFreeSpread(bestPrices)
                : GetArbitrageFreeSpread(
                    ImmutableDictionary.CreateRange(bestPrices.Where(p => p.Key == primaryOrderbook.ExchangeName)));
            var primaryBestPrices = bestPrices[primaryOrderbook.ExchangeName];
            var bidShift = arbitrageFreeSpread.WorstBid - primaryBestPrices.BestBid; // negative
            var askShift = arbitrageFreeSpread.WorstAsk - primaryBestPrices.BestAsk; // positive
            var volumeMultiplier =
                _extPricesSettingsService.GetVolumeMultiplier(primaryOrderbook.AssetPairId,
                    primaryOrderbook.ExchangeName);
            var priceMarkups = _extPricesSettingsService.GetPriceMarkups(primaryOrderbook.AssetPairId);
            return Transform(primaryOrderbook, bidShift + priceMarkups.Bid, askShift + priceMarkups.Ask,
                volumeMultiplier);
        }

        private Orderbook Transform(Orderbook orderbook, decimal bidShift, decimal askShift,
            decimal volumeMultiplier)
        {
            var roundFunc = _priceRoundingService.GetRoundFunc(orderbook.AssetPairId);
            return new Orderbook(
                orderbook.AssetPairId,
                orderbook.Bids.Select(b => 
                        new OrderbookPosition(roundFunc(b.Price + bidShift), b.Volume * volumeMultiplier))
                    .ToImmutableArray(),
                orderbook.Asks.Select(b =>
                        new OrderbookPosition(roundFunc(b.Price + askShift), b.Volume * volumeMultiplier))
                    .ToImmutableArray());
        }

        private static (decimal WorstBid, decimal WorstAsk) GetArbitrageFreeSpread(
            IReadOnlyDictionary<string, BestPrices> bestPrices)
        {
            var worstBid = bestPrices.Values.Min(p => p.BestBid);
            var worstAsk = bestPrices.Values.Max(p => p.BestAsk);
            if (worstBid >= worstAsk)
            {
                worstBid = worstAsk - 0.00000001m; // hello crutches
            }

            return (worstBid, worstAsk);
        }
    }
}