using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class AggregateOrderbookService : IAggregateOrderbookService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        public AggregateOrderbookService(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
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

        private static ImmutableArray<OrderbookPosition> Aggregate(AggregateOrderbookSettings settings,
            ImmutableArray<OrderbookPosition> positions)
        {
            if (settings.AsIsLevelsCount >= positions.Length)
                return positions;

            var aggregatedLevels = new Stack<decimal>(settings.CumulativeVolumeLevels.Reverse());
            var result = ImmutableArray.CreateBuilder<OrderbookPosition>(settings.AsIsLevelsCount
                                                                         + aggregatedLevels.Count);
            result.AddRange(positions.Take(settings.AsIsLevelsCount));

            if (aggregatedLevels.Count == 0)
                return result.ToImmutable();

            var asIsOrdersVolume = result.Sum(p => p.Volume);
            var cumulativeVolume = asIsOrdersVolume;
            var currentVolume = 0m;
            var currentLimit = aggregatedLevels.Pop();
            foreach (var position in positions.Skip(settings.AsIsLevelsCount))
            {
                cumulativeVolume += position.Volume;
                currentVolume += position.Volume;
                if (cumulativeVolume >= currentLimit)
                {
                    if (currentVolume > 0 && cumulativeVolume - position.Volume > asIsOrdersVolume)
                    {
                        result.Add(new OrderbookPosition(position.Price, currentVolume));
                        currentVolume = 0;
                    }

                    if (!aggregatedLevels.TryPop(out currentLimit))
                        break;
                }
            }

            return result.ToImmutable();
        }
    }
}