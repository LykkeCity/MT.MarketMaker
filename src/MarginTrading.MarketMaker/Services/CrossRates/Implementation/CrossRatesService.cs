using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRatesService : ICrossRatesService
    {
        private readonly ReadWriteLockedDictionary<string, Orderbook> _orderbooks
            = new ReadWriteLockedDictionary<string, Orderbook>();

        private readonly IBestPricesService _bestPricesService;
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;

        public CrossRatesService(IBestPricesService bestPricesService,
            ICrossRateCalcInfosService crossRateCalcInfosService)
        {
            _bestPricesService = bestPricesService;
            _crossRateCalcInfosService = crossRateCalcInfosService;
        }

        [ItemNotNull]
        public ImmutableList<Orderbook> CalcDependentOrderbooks([NotNull] Orderbook orderbook) // ex: (btcusd)
        {
            _orderbooks[orderbook.AssetPairId] = orderbook
                                                 ?? throw new ArgumentNullException(nameof(orderbook));
            var dependent = _crossRateCalcInfosService.GetDependentAssetPairs(orderbook.AssetPairId); // ex: btceur
            return dependent.Select(CalculateOrderbook).Where(o => o != null).ToImmutableList();
        }

        [CanBeNull]
        private Orderbook CalculateOrderbook(CrossRateCalcInfo info)
        {
            var sourceOrderbook1 = _orderbooks.GetValueOrDefault(info.Source1.Id); // ex: btcusd
            var sourceOrderbook2 = _orderbooks.GetValueOrDefault(info.Source2.Id); // ex: eurusd
            if (sourceOrderbook1 == null || sourceOrderbook2 == null)
            {
                Trace.Write(info.ResultingPairId + " warn trace",
                    "Skipping generating cross-rate: " +
                    (sourceOrderbook1 == null ? $"Orderbook for {info.Source1.Id} not exists. " : "") +
                    (sourceOrderbook2 == null ? $"Orderbook for {info.Source2.Id} not exists. " : ""));
                return null;
            }

            var bestPrices1 = _bestPricesService.Calc(sourceOrderbook1); // ex: btcusd
            var bestPrices2 = _bestPricesService.Calc(sourceOrderbook2); // ex: eurusd
            var crossBid = GetCrossRate(bestPrices1.BestBid, bestPrices2.BestBid, info); // todo: if revert - get ask
            var crossAsk = GetCrossRate(bestPrices1.BestAsk, bestPrices2.BestAsk, info);
            return new Orderbook(info.ResultingPairId,
                ImmutableArray.Create(new OrderbookPosition(crossBid, 1)), // in future: calc whole orderbook
                ImmutableArray.Create(new OrderbookPosition(crossAsk, 1)));
        }

        private static decimal GetCrossRate(decimal rate1, decimal rate2, CrossRateCalcInfo info)
        {
            if (!info.Source1.IsTransitoryAssetQuoting)
                rate1 = 1 / rate1;

            if (!info.Source2.IsTransitoryAssetQuoting)
                rate2 = 1 / rate2;

            return rate1 / rate2;
        }
    }
}