using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class DependentCrossRatesService : IDependentCrossRatesService
    {
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;
        private readonly IAssetPairsInfoService _assetPairsInfoService;
        private readonly ICachedCalculation<ImmutableHashSet<(string, string)>> _configuredCrossPairs;
        private readonly ICachedCalculation<ILookup<string, CrossRateCalcInfo>> _existingAssetPairs;

        public DependentCrossRatesService(ICrossRatesSettingsService crossRatesSettingsService,
            IAssetPairsInfoService assetPairsInfoService)
        {
            _crossRatesSettingsService = crossRatesSettingsService;
            _assetPairsInfoService = assetPairsInfoService;
            _configuredCrossPairs = GetConfiguredCrossPairs();
            _existingAssetPairs = GetExistingAssetPairs();
        }

        [ItemNotNull]
        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs([NotNull] string assetPairId) // ex: (btcusd)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _existingAssetPairs.Get()[assetPairId].RequiredNotNullElems("result"); // ex: btceur
        }

        public IEnumerable<string> GetExistingCrossPairs()
        {
            return _existingAssetPairs.Get().SelectMany(gr => gr).Select(i => i.ResultingPairId).Distinct();
        }

        public CrossRateCalcInfo CalculateDefault(string assetPairId)
        {
            var assetPairs = _assetPairsInfoService.Get();
            var resultingAssetPair = assetPairs.GetValueOrDefault(assetPairId);
            if (resultingAssetPair == null)
                return null;
            
            return GetCrossRateCalcInfo(resultingAssetPair, assetPairs);
        }

        private ICachedCalculation<ILookup<string, CrossRateCalcInfo>> GetExistingAssetPairs()
        {
            return Calculate.Cached(
                () => new
                {
                    AllPairs = _assetPairsInfoService.Get(), /* ex: [btceur, btcusd, eurusd] */
                    CrossPairs = _configuredCrossPairs.Get() /* ex: [(btc, eur), (eur, btc)] */
                },
                (o, n) => o.AllPairs == n.AllPairs && o.CrossPairs == n.CrossPairs,
                s => s.AllPairs.Values
                    .Where(p => s.CrossPairs.Contains((p.BaseAssetId, p.QuotingAssetId)) &&
                                !string.IsNullOrWhiteSpace(p.Source) &&
                                !string.IsNullOrWhiteSpace(p.Source2)) // ex: btceur
                    .Select(p => GetCrossRateCalcInfo(p, s.AllPairs)) // ex: {btceur, btcusd, eurusd}
                    .SelectMany(i => new[]
                        {(i.Source1.Id, i), (i.Source2.Id, i)}) // ex: [(btcusd, btceur), (eurusd, btceur)]
                    .ToLookup() // ex: [btcusd=>btceur, eurusd=>btceur]
            );
        }

        private ICachedCalculation<ImmutableHashSet<(string, string)>> GetConfiguredCrossPairs()
        {
            return Calculate.Cached(() => _crossRatesSettingsService.Get(),
                ReferenceEquals,
                settings => settings
                    .SelectMany(s => s.OtherAssetsIds.SelectMany(
                        o => new[] {(s.BaseAssetId, o), (o, s.BaseAssetId)})) // [(btc, eur), (eur, btc)]
                    .ToImmutableHashSet());
        }

        private static CrossRateCalcInfo GetCrossRateCalcInfo(AssetPairInfo resultingPair, IReadOnlyDictionary<string, AssetPairInfo> assetPairs)
        {
            var sourcePair1 = assetPairs.GetValueOrDefault(resultingPair.Source);
            var sourcePair2 = assetPairs.GetValueOrDefault(resultingPair.Source2);
            if (sourcePair1 == null || sourcePair2 == null)
                return null;
            
            var baseAssetId = GetBaseCrossRateAsset(sourcePair1, sourcePair2);
            return new CrossRateCalcInfo(resultingPair.Id, new CrossRateSourceAssetPair(resultingPair.Source, sourcePair1.QuotingAssetId == baseAssetId),
                new CrossRateSourceAssetPair(resultingPair.Source2, sourcePair2.QuotingAssetId == baseAssetId));
        }

        /// <summary>
        /// Base asset is the one that is common in two source pairs used for cross-rate calculating.<br/>
        /// Ex: ETHUSD is calculated based on BTC from ETHBTC and BTCUSD.
        /// </summary>
        private static string GetBaseCrossRateAsset(AssetPairInfo sourcePair1, AssetPairInfo sourcePair2)
        {
            var sourceAssets1 = new[] { sourcePair1.BaseAssetId, sourcePair1.QuotingAssetId };
            var sourceAssets2 = new[] { sourcePair2.BaseAssetId, sourcePair2.QuotingAssetId };
            return sourceAssets1.Single(a => sourceAssets2.Contains(a));
        }
    }
}