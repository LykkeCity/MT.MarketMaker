using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRateCalcInfosService : ICrossRateCalcInfosService
    {
        private readonly ISettingsRootService _settingsRootService;
        private readonly ICachedCalculation<ILookup<string, CrossRateCalcInfo>> _dependentAssetPairs;


        public CrossRateCalcInfosService(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
            _dependentAssetPairs = DependentAssetPairsCache();
        }

        public void Add(CrossRateCalcInfo info)
        {
            _settingsRootService.Add(info.ResultingPairId, new AssetPairSettings(AssetPairQuotesSourceTypeEnum.CrossRates, old.ExtPriceSettings, info));
        }

        public void Update([NotNull] CrossRateCalcInfo info)
        {
            _settingsRootService.Update(info.ResultingPairId,
                old => new AssetPairSettings(old.QuotesSourceType, old.ExtPriceSettings, info));
        }

        public IReadOnlyList<CrossRateCalcInfo> Get()
        {
        }

        [ItemNotNull]
        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _dependentAssetPairs.Get()[assetPairId].RequiredNotNullElems("result");
        }

        public CrossRateCalcInfo GetDefault()
        {

        }

        public CrossRateCalcInfo Get(string assetPairId)
        {
            throw new NotImplementedException();
        }

        private ICachedCalculation<ILookup<string, CrossRateCalcInfo>> DependentAssetPairsCache()
        {
            return Calculate.Cached(Get, ReferenceEquals,
                src => src.SelectMany(i => new[] { (i.Source1.Id, i), (i.Source2.Id, i) })
                    .ToLookup());
        }
    }
}