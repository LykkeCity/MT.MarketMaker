using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
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

        public AssetPairSourceTypeService(ISettingsRootService settingsRootService,
            IExtPricesSettingsService extPricesSettingsService, ICrossRateCalcInfosService crossRateCalcInfosService)
        {
            _settingsRootService = settingsRootService;
            _extPricesSettingsService = extPricesSettingsService;
            _crossRateCalcInfosService = crossRateCalcInfosService;
        }

        public void AddAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeDomainEnum sourceType)
        {
            _settingsRootService.Add(assetPairId,
                new AssetPairSettings(sourceType, _extPricesSettingsService.GetDefaultExtPriceSettings(),
                    _crossRateCalcInfosService.GetDefault(assetPairId),
                    _extPricesSettingsService.GetDefaultAggregateOrderbookSettings()));
        }

        public void UpdateAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeDomainEnum sourceType)
        {
            _settingsRootService.Update(assetPairId,
                old => new AssetPairSettings(sourceType, old.ExtPriceSettings,
                    old.CrossRateCalcInfo, old.AggregateOrderbookSettings));
        }

        public ImmutableDictionary<string, AssetPairQuotesSourceTypeDomainEnum> Get()
        {
            return _settingsRootService.Get().AssetPairs
                .ToImmutableDictionary(s => s.Key, s => s.Value.QuotesSourceType);
        }

        public AssetPairQuotesSourceTypeDomainEnum? Get(string assetPairId)
        {
            return _settingsRootService.Get(assetPairId)?.QuotesSourceType;
        }

        public ImmutableHashSet<string> GetPairsByQuotesSourceType(AssetPairQuotesSourceTypeDomainEnum quotesSourceType)
        {
            return _settingsRootService.Get().AssetPairs.Where(p => p.Value.QuotesSourceType == quotesSourceType)
                .Select(p => p.Key).ToImmutableHashSet();
        }

        public void Delete(string assetPairId)
        {
            _settingsRootService.Delete(assetPairId);
        }
    }
}