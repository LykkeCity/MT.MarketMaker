using System;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class OutdatedOrderbooksService : IOutdatedOrderbooksService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        public OutdatedOrderbooksService(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
        }

        public bool IsOutdated(ExternalOrderbook orderbook, DateTime now)
        {
            var age = now - orderbook.LastUpdatedTime;
            var threshold = _extPricesSettingsService.GetOrderbookOutdatingThreshold(orderbook.AssetPairId, orderbook.ExchangeName, now);
            return age > threshold;
        }
    }
}