using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure.Implemetation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class DependentCrossRatesService : IDependentCrossRatesService
    {
        private readonly IAssetsservice _assetsService;
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;

        public DependentCrossRatesService(IAssetsservice assetsService,
            ICrossRatesSettingsService crossRatesSettingsService)
        {
            _assetsService = assetsService;
            _crossRatesSettingsService = crossRatesSettingsService;
        }

        [ItemNotNull]
        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs([NotNull] string assetPairId) // ex: (btcusd)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            var configuredCrossPairs = GetConfiguredCrossPairs(); // ex: [(btc, eur), (eur, btc)]
            var assetPairs =
                _assetsService.GetAssetPairs().Where(p => !p.IsDisabled).ToDictionary(p => p.Id); // todo: cache
            var existingAssetPairs = assetPairs.Values
                .Where(p => configuredCrossPairs.Contains((p.BaseAssetId, p.QuotingAssetId)) &&
                            !string.IsNullOrWhiteSpace(p.Source) && !string.IsNullOrWhiteSpace(p.Source2)) // ex: btceur
                .Select(p => GetCrossRateCalcInfo(p, assetPairs)) // ex: {btceur, btcusd, eurusd}
                .SelectMany(i => new[]
                    {(i.Source1.Id, i), (i.Source2.Id, i)}) // ex: [(btcusd, btceur), (eurusd, btceur)]
                .ToLookup(); // ex: [btcusd=>btceur, eurusd=>btceur] // todo: cache
            return existingAssetPairs[assetPairId].RequiredNotNullElems("result"); // ex: btceur
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
            var sourceAssets1 = new[] {sourcePair1.BaseAssetId, sourcePair1.QuotingAssetId };
            var sourceAssets2 = new[] {sourcePair2.BaseAssetId, sourcePair2.QuotingAssetId };
            return sourceAssets1.Single(a => sourceAssets2.Contains(a));
        }

        private ImmutableHashSet<(string, string)> GetConfiguredCrossPairs()
        {
            var crossRatesSettingsModel = _crossRatesSettingsService.Get(); // [btc], [eur]
            return crossRatesSettingsModel.BaseAssetsIds
                .Cartesian(crossRatesSettingsModel.OtherAssetsIds,
                    (a1, a2) => new[] {(a1, a2), (a2, a1)}) // [(btc, eur), (eur, btc)]
                .SelectMany(a => a)
                .ToImmutableHashSet(); // todo: cache
        }
    }
}