using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper;
using Common;
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

        public IReadOnlyList<ExtPriceStatusModel> Get()
        {
            var qualities = _primaryExchangeService.GetQualities();
            var primaryExchanges = _primaryExchangeService.GetLastPrimaryExchanges();
            var bestPrices = _bestPricesService.GetLastCalculated();

            return qualities
                .SelectMany(asset =>
                {
                    var primary1 = primaryExchanges.GetValueOrDefault(asset.Key);
                    return asset.Value.Select(exchange =>
                    {
                        bestPrices.TryGetValue((asset.Key, exchange.Key), out var bestPrice1);
                        return Convert(asset.Key, exchange.Key, exchange.Value, primary1 == exchange.Key, bestPrice1);
                    });
                })
                .OrderBy(q => q.AssetPairId)
                .ThenBy(q => q.ExchangeName)
                .ToList();
        }

        public IReadOnlyList<ExtPriceStatusModel> Get(string assetPairId)
        {
            return Get().Where(m => m.AssetPairId == assetPairId).ToList();
        }

        private ExtPriceStatusModel Convert(string assetPairId, string exchangeName, ExchangeQuality exchangeQuality, bool isPrimary, BestPrices bestPrice)
        {
            var model = _convertService.Convert<ExchangeQuality, ExtPriceStatusModel>(exchangeQuality,
                o => o.ConfigureMap(MemberList.Source));
            model.AssetPairId = assetPairId;
            model.ExchangeName = exchangeName;
            model.IsPrimary = isPrimary;
            model.BestPrices = _convertService.Convert<BestPrices, BestPricesModel>(bestPrice);
            return model;
        }
    }
}