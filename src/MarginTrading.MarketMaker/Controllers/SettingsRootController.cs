using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class SettingsRootController : Controller
    {
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;
        private readonly IAssetPairsSettingsService _assetPairsSettingsService;
        private readonly IPriceCalcSettingsService _priceCalcSettingsService;

        public SettingsRootController(ICrossRateCalcInfosService crossRateCalcInfosService,
            IAssetPairsSettingsService assetPairsSettingsService, IPriceCalcSettingsService priceCalcSettingsService)
        {
            _crossRateCalcInfosService = crossRateCalcInfosService ??
                                         throw new ArgumentNullException(nameof(crossRateCalcInfosService));
            _assetPairsSettingsService = assetPairsSettingsService ??
                                         throw new ArgumentNullException(nameof(assetPairsSettingsService));
            _priceCalcSettingsService = priceCalcSettingsService ??
                                        throw new ArgumentNullException(nameof(priceCalcSettingsService));
        }

        [HttpGet]
        public async Task<SettingsRoot> Get()
        {
            var assetPairs = (await _assetPairsSettingsService.GetAllPairsSourcesAsync())
                .ToImmutableDictionary(s => s.AssetPairId, MakeAssetPairSettings);
            return new SettingsRoot(assetPairs);
        }

        private AssetPairSettings MakeAssetPairSettings(Models.Api.AssetPairSettings s)
        {
            var ext = _priceCalcSettingsService.GetAllAsync(s.AssetPairId).GetAwaiter().GetResult().FirstOrDefault();
            var extGenerated = ext == null
                ? GetDefaultExtSettings()
                : new AssetPairExtPriceSettings(
                    ext.PresetDefaultExchange ?? ext.Exchanges.FirstOrDefault()?.Exchange ?? "",
                    (decimal) ext.OutlierThreshold,
                    ext.MinOrderbooksSendingPeriod ?? TimeSpan.FromSeconds(0.5),
                    new AssetPairMarkupsParams(ext.Markups.Bid, ext.Markups.Ask),
                    new RepeatedOutliersParams(ext.RepeatedOutliers.MaxSequenceLength,
                        ext.RepeatedOutliers.MaxSequenceAge, ext.RepeatedOutliers.MaxAvg,
                        ext.RepeatedOutliers.MaxAvgAge),
                    ext.Steps, ext.Exchanges.ToImmutableDictionary(e => e.Exchange, e =>
                        new ExchangeExtPriceSettings(e.OrderbookOutdatingThreshold,
                            new ExchangeDisabledSettings(e.Disabled.IsTemporarilyDisabled, e.Disabled.Reason ?? ""),
                            new ExchangeHedgingSettings((decimal) e.Hedging.DefaultPreference,
                                e.Hedging.IsTemporarilyUnavailable),
                            new ExchangeOrderGenerationSettings(e.OrderGeneration.VolumeMultiplier,
                                e.OrderGeneration.OrderRenewalDelay))));
            var cross = _crossRateCalcInfosService.Get().FirstOrDefault() ?? new CrossRateCalcInfo("",
                            new CrossRateSourceAssetPair("", false), new CrossRateSourceAssetPair("", false));
            return new AssetPairSettings(s.QuotesSourceType,
                extGenerated,
                new CrossRateCalcInfo(cross.ResultingPairId,
                    new CrossRateSourceAssetPair(cross.Source1.Id, cross.Source1.IsTransitoryAssetQuoting),
                    new CrossRateSourceAssetPair(cross.Source2.Id, cross.Source2.IsTransitoryAssetQuoting)));
        }

        private AssetPairExtPriceSettings GetDefaultExtSettings()
        {
            return new AssetPairExtPriceSettings("",
                0.05m, TimeSpan.FromSeconds(0.5), new AssetPairMarkupsParams(0, 0),
                new RepeatedOutliersParams(10, TimeSpan.FromMinutes(5), 10,
                    TimeSpan.FromMinutes(5)),
                GetDefaultSteps(), ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty);
        }
        
        private static ImmutableDictionary<OrderbookGeneratorStepEnum, bool> GetDefaultSteps()
        {
            return Enum.GetValues(typeof(OrderbookGeneratorStepEnum)).Cast<OrderbookGeneratorStepEnum>()
                .ToImmutableDictionary(e => e, e => true);
        }
    }
}