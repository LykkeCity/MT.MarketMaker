using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MarginTrading.MarketMaker.Services.ExtPrices;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRatesService : ICrossRatesService
    {
        private readonly ReadWriteLockedDictionary<string, Orderbook> _orderbooks
            = new ReadWriteLockedDictionary<string, Orderbook>();

        private readonly IBestPricesService _bestPricesService;
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;

        public CrossRatesService(IBestPricesService bestPricesService,
            ICrossRateCalcInfosService crossRateCalcInfosService,
            IAssetPairSourceTypeService assetPairSourceTypeService)
        {
            _bestPricesService = bestPricesService;
            _crossRateCalcInfosService = crossRateCalcInfosService;
            _assetPairSourceTypeService = assetPairSourceTypeService;
        }

        [ItemNotNull]
        public ImmutableList<Orderbook> CalcDependentOrderbooks([NotNull] Orderbook orderbook) // ex: (btcusd)
        {
            _orderbooks[orderbook.AssetPairId] = orderbook
                                                 ?? throw new ArgumentNullException(nameof(orderbook));
            var dependent = _crossRateCalcInfosService.GetDependentAssetPairs(orderbook.AssetPairId)
                .Where(p => _assetPairSourceTypeService.Get(p.ResultingPairId) ==
                            AssetPairQuotesSourceTypeDomainEnum.CrossRates); // ex: btceur
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
            var crossBid = GetCrossRate(bestPrices1, bestPrices2, info, true);
            var crossAsk = GetCrossRate(bestPrices1, bestPrices2, info, false);
            return new Orderbook(info.ResultingPairId,
                ImmutableArray.Create(new OrderbookPosition(crossBid, 1)), // in future: calc whole orderbook
                ImmutableArray.Create(new OrderbookPosition(crossAsk, 1)));
        }

        private static decimal GetCrossRate(BestPrices bba1, BestPrices bba2, CrossRateCalcInfo info, bool bid)
        {
            var first = GetRate(bba1, bid, !info.Source1.IsTransitoryAssetQuoting);
            var second = GetRate(bba2, bid, info.Source2.IsTransitoryAssetQuoting);
            return first * second;
        }

        private static decimal GetRate(BestPrices bestPrices, bool bid, bool invert)
        {
            var needBid = invert ? !bid : bid;
            var nonInverted = needBid ? bestPrices.BestBid : bestPrices.BestAsk;
            return invert ? 1 / nonInverted : nonInverted;
        }
    }
}