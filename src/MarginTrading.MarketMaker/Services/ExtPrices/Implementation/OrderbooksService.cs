using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class OrderbooksService : IOrderbooksService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        private readonly ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExternalOrderbook>> _orderbooks =
            new ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExternalOrderbook>>();

        public OrderbooksService(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
        }

        public ImmutableDictionary<string, ExternalOrderbook> AddAndGetByAssetPair(ExternalOrderbook orderbook)
        {
            var existingExchanges = _extPricesSettingsService.Get(orderbook.AssetPairId).Exchanges.Keys;
            return _orderbooks.AddOrUpdate(orderbook.AssetPairId,
                k => ImmutableDictionary.Create<string, ExternalOrderbook>().Add(orderbook.ExchangeName, orderbook),
                (k, dict) => AddOrderbookAndFilterByExchangeExistance(orderbook, dict, existingExchanges));
        }

        private static ImmutableDictionary<string, ExternalOrderbook> AddOrderbookAndFilterByExchangeExistance(
            ExternalOrderbook orderbook, ImmutableDictionary<string, ExternalOrderbook> dict,
            IEnumerable<string> existingExchanges)
        {
            dict = dict.SetItem(orderbook.ExchangeName, orderbook);
            foreach (var exchangeToDelete in dict.Keys.Except(existingExchanges))
            {
                dict = dict.Remove(exchangeToDelete);
            }
            
            return dict;
        }
    }
}