using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class AggregateOrderbookService : IAggregateOrderbookService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly ISystem _system;

        public AggregateOrderbookService(IExtPricesSettingsService extPricesSettingsService, ISystem system)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _system = system;
        }

        public Orderbook Aggregate(Orderbook originalOrderbook)
        {
            if (!_extPricesSettingsService.IsStepEnabled(
                OrderbookGeneratorStepDomainEnum.AggregateOrderbook,
                originalOrderbook.AssetPairId))
                return originalOrderbook;

            var settings = _extPricesSettingsService.GetAggregateOrderbookSettings(originalOrderbook.AssetPairId);

            return new Orderbook(originalOrderbook.AssetPairId,
                Aggregate(settings, originalOrderbook.Bids),
                Aggregate(settings, originalOrderbook.Asks));
        }

        private ImmutableArray<OrderbookPosition> Aggregate(AggregateOrderbookSettings settings,
            ImmutableArray<OrderbookPosition> positions)
        {
            if (settings.AsIsLevelsCount >= positions.Length)
                return positions;

            var r = _system.GetRandom();
            var aggregatedLevels = new Stack<decimal>(settings.CumulativeVolumeLevels.Reverse()
                .Select(n => n * (1 + ((decimal) r.NextDouble() - 0.5m) * settings.RandomFraction)));
            var result = ImmutableArray.CreateBuilder<OrderbookPosition>(settings.AsIsLevelsCount
                                                                         + aggregatedLevels.Count); 
            result.AddRange(positions.Take(settings.AsIsLevelsCount));

            var prevLevelCumulativeVolume = result.Sum(p => p.Volume);
            if (!TryGetNewLimit(aggregatedLevels, prevLevelCumulativeVolume, out var currentLimit))
                return result.ToImmutable();

            var currentVolume = 0m;
            var cumulativeVolume = prevLevelCumulativeVolume;
            foreach (var position in positions.Skip(settings.AsIsLevelsCount))
            {
                cumulativeVolume += position.Volume;
                currentVolume += position.Volume;
                if (cumulativeVolume >= currentLimit)
                {
                    result.Add(new OrderbookPosition(position.Price, currentVolume));
                    prevLevelCumulativeVolume = cumulativeVolume;
                    currentVolume = 0;
                    if (!TryGetNewLimit(aggregatedLevels, prevLevelCumulativeVolume, out currentLimit))
                        break;
                }
            }

            return result.ToImmutable();
        }

        private static bool TryGetNewLimit(Stack<decimal> aggregatedLevels, decimal prevLevelCumulativeVolume,
            out decimal currentLimit)
        {
            do
            {
                var gotLevel = aggregatedLevels.TryPop(out  currentLimit);
                if (!gotLevel)
                    return false;
                
            } while (currentLimit <= prevLevelCumulativeVolume);
            return true;
        }
    }
}