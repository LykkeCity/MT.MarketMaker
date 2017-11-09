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

        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId) // ex: (btcusd)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            var configuredCrossPairs = GetConfiguredCrossPairs(); // ex: [(btc, eur), (eur, btc)]
            var assetPairs = _assetsService.GetAssetPairs().Where(p => !p.IsDisabled).ToDictionary(p => p.Id); // todo: cache
            var existingAssetPairs = assetPairs.Values
                .Where(p => configuredCrossPairs.Contains((p.BaseAssetId, p.QuotingAssetId)) && !string.IsNullOrWhiteSpace(p.Source) && !string.IsNullOrWhiteSpace(p.Source2)) // ex: btceur
                .Select(p => new CrossRateCalcInfo(p.Id, GetCrossRateSourceAssetPair(p.Source, assetPairs),
                    GetCrossRateSourceAssetPair(p.Source2, assetPairs))) // ex: {btceur, btcusd, eurusd}
                .SelectMany(i => new[]
                    {(i.Source1.Id, i), (i.Source2.Id, i)}) // ex: [(btcusd, btceur), (eurusd, btceur)]
                .ToLookup(); // ex: [btcusd=>btceur, eurusd=>btceur] // todo: cache
            return existingAssetPairs[assetPairId].RequiredNotNullOrEmpty("result"); // ex: btceur
        }

        private static CrossRateSourceAssetPair GetCrossRateSourceAssetPair(string pairId,
            Dictionary<string, AssetPairResponseModel> assetPairs)
        {
            return new CrossRateSourceAssetPair(pairId, assetPairs[pairId].QuotingAssetId == "USD");
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