using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implemetation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRatesService : ICrossRatesService
    {
        private readonly ReadWriteLockedDictionary<string, Orderbook> _orderbooks
            = new ReadWriteLockedDictionary<string, Orderbook>();

        private readonly IBestPricesService _bestPricesService;
        private readonly IDependentCrossRatesService _dependentCrossRatesService;

        public CrossRatesService(IBestPricesService bestPricesService,
            IDependentCrossRatesService dependentCrossRatesService)
        {
            _bestPricesService = bestPricesService;
            _dependentCrossRatesService = dependentCrossRatesService;
        }

        public ImmutableList<Orderbook> CalcDependentOrderbooks(Orderbook orderbook) // ex: (btcusd)
        {
            _orderbooks[orderbook.AssetPairId] = orderbook;
            var dependent = _dependentCrossRatesService.GetDependentAssetPairs(orderbook.AssetPairId); // ex: btceur
            return dependent.Select(CalculateOrderbook).Where(o => o != null).ToImmutableList();
        }

        [CanBeNull]
        private Orderbook CalculateOrderbook(CrossRateCalcInfo info)
        {
            var sourceOrderbook1 = _orderbooks.GetValueOrDefault(info.Source1.Id); // ex: btcusd
            var sourceOrderbook2 = _orderbooks.GetValueOrDefault(info.Source2.Id); // ex: eurusd
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (sourceOrderbook1 == null || sourceOrderbook2 == null)
            {
                Trace.Write($"Skipping gernerating cross");
                return null;
            }

            var bestPrices1 = _bestPricesService.Calc(sourceOrderbook1); // ex: btcusd
            var bestPrices2 = _bestPricesService.Calc(sourceOrderbook2); // ex: eurusd
            var crossBid = GetCrossRate(bestPrices1.BestBid, bestPrices2.BestBid, info);
            var crossAsk = GetCrossRate(bestPrices1.BestAsk, bestPrices2.BestAsk, info);
            return new Orderbook(info.Source1.Id + info.Source2.Id,
                ImmutableArray.Create(new OrderbookPosition(crossBid, 1)), // in future: calc whole orderbook
                ImmutableArray.Create(new OrderbookPosition(crossAsk, 1)));
        }

        // todo: reduce operations to speedup
        private static decimal GetCrossRate(decimal rate1, decimal rate2, CrossRateCalcInfo info)
        {
            if (!info.Source1.IsUsdQuoting)
                rate1 = 1 / rate1;

            if (!info.Source2.IsUsdQuoting)
                rate2 = 1 / rate2;

            return rate1 / rate2;
        }
    }
}