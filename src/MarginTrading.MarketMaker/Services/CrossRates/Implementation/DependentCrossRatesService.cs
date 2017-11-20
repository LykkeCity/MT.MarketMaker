using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class DependentCrossRatesService : IDependentCrossRatesService
    {
        private readonly IAssetsService _assetsService;
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;
        private readonly ISystem _system;
        private readonly ICachedCalculation<Dictionary<string, AssetPairResponseModel>> _assetPairs;
        private readonly ICachedCalculation<ImmutableHashSet<(string, string)>> _configuredCrossPairs;
        private readonly ICachedCalculation<ILookup<string, CrossRateCalcInfo>> _existingAssetPairs;

        public DependentCrossRatesService(IAssetsService assetsService,
            ICrossRatesSettingsService crossRatesSettingsService, ISystem system)
        {
            _assetsService = assetsService;
            _crossRatesSettingsService = crossRatesSettingsService;
            _system = system;
            _assetPairs = GetAssetPairs();
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

        private ICachedCalculation<ILookup<string, CrossRateCalcInfo>> GetExistingAssetPairs()
        {
            return Calculate.Cached(
                () => new
                {
                    AllPairs = _assetPairs.Get(), /* ex: [btceur, btcusd, eurusd] */
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

        private ICachedCalculation<Dictionary<string, AssetPairResponseModel>> GetAssetPairs()
        {
            return Calculate.Cached(() => _system.UtcNow,
                (prev, now) => now.Subtract(prev) < TimeSpan.FromMinutes(5),
                now => _assetsService.GetAssetPairs().Where(p => !p.IsDisabled).ToDictionary(p => p.Id));
        }

        private static CrossRateCalcInfo GetCrossRateCalcInfo(AssetPairResponseModel resultingPair, Dictionary<string, AssetPairResponseModel> assetPairs)
        {
            var sourcePair1 = assetPairs[resultingPair.Source];
            var sourcePair2 = assetPairs[resultingPair.Source2];
            var baseAssetId = GetBaseCrossRateAsset(sourcePair1, sourcePair2);
            return new CrossRateCalcInfo(resultingPair.Id, new CrossRateSourceAssetPair(resultingPair.Source, sourcePair1.QuotingAssetId == baseAssetId),
                new CrossRateSourceAssetPair(resultingPair.Source2, sourcePair2.QuotingAssetId == baseAssetId));
        }

        /// <summary>
        /// Base asset is the one that is common in two source pairs used for cross-rate calculating.<br/>
        /// Ex: ETHUSD is calculated based on BTC from ETHBTC and BTCUSD.
        /// </summary>
        private static string GetBaseCrossRateAsset(AssetPairResponseModel sourcePair1, AssetPairResponseModel sourcePair2)
        {
            var sourceAssets1 = new[] { sourcePair1.BaseAssetId, sourcePair1.QuotingAssetId };
            var sourceAssets2 = new[] { sourcePair2.BaseAssetId, sourcePair2.QuotingAssetId };
            return sourceAssets1.Single(a => sourceAssets2.Contains(a));
        }
    }
}