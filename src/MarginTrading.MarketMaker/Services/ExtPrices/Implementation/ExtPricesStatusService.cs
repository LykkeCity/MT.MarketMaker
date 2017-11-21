using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class ExtPricesStatusService : IExtPricesStatusService
    {
        private readonly IPrimaryExchangeService _primaryExchangeService;
        private readonly IBestPricesService _bestPricesService;

        public ExtPricesStatusService(IPrimaryExchangeService primaryExchangeService,
            IBestPricesService bestPricesService)
        {
            _primaryExchangeService = primaryExchangeService;
            _bestPricesService = bestPricesService;
        }

        public IReadOnlyDictionary<string, IReadOnlyList<ExtPriceStatusModel>> Get()
        {
            var qualities = _primaryExchangeService.GetQualities();
            var primaryExchanges = _primaryExchangeService.GetLastPrimaryExchanges();
            var result = qualities.ToDictionary(pair => pair.Key,
                pair =>
                {
                    var primary = primaryExchanges.GetValueOrDefault(pair.Key);
                    return (IReadOnlyList<ExtPriceStatusModel>)pair.Value.Select(p => Convert(p.Key, p.Value, primary == p.Key)).ToList();
                });

            var bestPrices = _bestPricesService.GetLastCalculated();
            foreach (var asset in result)
            {
                foreach (var exchange in asset.Value)
                {
                    if (bestPrices.TryGetValue((asset.Key, exchange.Exchange), out var bestPrice))
                    {
                        exchange.BestPrices = Convert(bestPrice);
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<ExtPriceStatusModel> Get(string assetPairId)
        {
            return Get().GetValueOrDefault(assetPairId, ImmutableArray<ExtPriceStatusModel>.Empty);
        }

        private BestPricesModel Convert(BestPrices exchangeName)
        {
            AUTOMAP
        }

        private ExtPriceStatusModel Convert(string exchangeName, ExchangeQuality exchangeQuality, bool isPrimary)
        {
            AUTOMAP
        }
    }
}