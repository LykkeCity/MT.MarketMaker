﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
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

        public void Update([NotNull] CrossRateCalcInfo info)
        {
            _settingsRootService.Update(info.ResultingPairId,
                old => new AssetPairSettings(old.QuotesSourceType, old.ExtPriceSettings, info, old.AggregateOrderbookSettings));
        }

        public ImmutableDictionary<string, CrossRateCalcInfo> Get()
        {
            return _settingsRootService.Get().AssetPairs
                .Where(s => s.Value.QuotesSourceType == AssetPairQuotesSourceTypeDomainEnum.CrossRates)
                .ToImmutableDictionary(s => s.Key, s => s.Value.CrossRateCalcInfo.RequiredNotNull(nameof(s.Value.CrossRateCalcInfo)));
        }

        [ItemNotNull]
        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _dependentAssetPairs.Get()[assetPairId].RequiredNotNullElems("result");
        }

        public CrossRateCalcInfo GetDefault(string assetPairId)
        {
            return new CrossRateCalcInfo(assetPairId, new CrossRateSourceAssetPair(string.Empty, false),
                new CrossRateSourceAssetPair(string.Empty, false));
        }

        public CrossRateCalcInfo Get(string assetPairId)
        {
            var crossRateCalcInfo = (_settingsRootService.Get(assetPairId)?.CrossRateCalcInfo);
            return string.IsNullOrWhiteSpace(crossRateCalcInfo?.ResultingPairId) ? null : crossRateCalcInfo;
        }

        private ICachedCalculation<ILookup<string, CrossRateCalcInfo>> DependentAssetPairsCache()
        {
            return Calculate.Cached(Get, ReferenceEquals,
                src => src.SelectMany(i => new[] {(i.Value.Source1.Id, i.Value), (i.Value.Source2.Id, i.Value)})
                    .ToLookup());
        }
    }
}