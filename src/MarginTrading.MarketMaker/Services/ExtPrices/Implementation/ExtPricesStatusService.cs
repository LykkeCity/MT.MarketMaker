using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class ExtPricesStatusService : IExtPricesStatusService
    {
        private readonly IPrimaryExchangeService _primaryExchangeService;
        private readonly IBestPricesService _bestPricesService;
        private readonly IConvertService _convertService;

        public ExtPricesStatusService(IPrimaryExchangeService primaryExchangeService,
            IBestPricesService bestPricesService, IConvertService convertService)
        {
            _primaryExchangeService = primaryExchangeService;
            _bestPricesService = bestPricesService;
            _convertService = convertService;
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
                    if (bestPrices.TryGetValue((asset.Key, exchange.ExchangeName), out var bestPrice))
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

        private BestPricesModel Convert(BestPrices bestPrices)
        {
            return _convertService.Convert<BestPrices, BestPricesModel>(bestPrices);
        }

        private ExtPriceStatusModel Convert(string exchangeName, ExchangeQuality exchangeQuality, bool isPrimary)
        {
            var model = _convertService.Convert<ExchangeQuality, ExtPriceStatusModel>(exchangeQuality,
                o => o.ConfigureMap(MemberList.Source));
            model.ExchangeName = exchangeName;
            model.IsPrimary = isPrimary;
            return model;
        }
    }
}