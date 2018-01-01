using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.ExtPrices;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    [UsedImplicitly]
    internal class AssetPairSourceTypeService : IAssetPairSourceTypeService
    {
        private readonly ISettingsRootService _settingsRootService;
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;

        public AssetPairSourceTypeService(ISettingsRootService settingsRootService, IExtPricesSettingsService extPricesSettingsService, ICrossRateCalcInfosService crossRateCalcInfosService)
        {
            _settingsRootService = settingsRootService;
            _extPricesSettingsService = extPricesSettingsService;
            _crossRateCalcInfosService = crossRateCalcInfosService;
        }

        public void AddAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            _settingsRootService.Add(assetPairId, new AssetPairSettings(sourceType, _extPricesSettingsService.GetDefault(), _crossRateCalcInfosService.GetDefault(assetPairId)));
        }

        public void UpdateAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            _settingsRootService.Update(assetPairId,
                old => new AssetPairSettings(sourceType, old.ExtPriceSettings,
                    old.CrossRateCalcInfo));
        }

        public ImmutableDictionary<string, AssetPairQuotesSourceTypeEnum> Get()
        {
            return _settingsRootService.Get().AssetPairs
                .ToImmutableDictionary(s => s.Key, s => s.Value.QuotesSourceType);
        }

        public AssetPairQuotesSourceTypeEnum? Get(string assetPairId)
        {
            return _settingsRootService.Get(assetPairId)?.QuotesSourceType;
        }

        public void Delete(string assetPairId)
        {
            _settingsRootService.Delete(assetPairId);
        }
    }
}