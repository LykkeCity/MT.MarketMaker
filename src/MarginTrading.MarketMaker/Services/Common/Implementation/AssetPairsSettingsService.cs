using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class AssetPairsSettingsService : IAssetPairsSettingsService
    {
        private readonly ISettingsRootService _settingsRootService;

        public AssetPairsSettingsService(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
        }

        public Task SetAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum assetPairQuotesSourceType)
        {
            var oldPairSettings = _settingsRootService.Get().AssetPairs.GetValueOrDefault(assetPairId);
            var pairSettings = new AssetPairSettings(assetPairQuotesSourceType, oldPairSettings?.ExtPriceSettings, oldPairSettings?.CrossRateCalcInfo);
            _settingsRootService.Set()
        }

        public async Task<List<AssetPairSettingsModel>> GetAllPairsSourcesAsync()
        {
            return (await _assetsPairsSettingsRepository.GetAllAsync())
                .Select(s => new AssetPairSettingsModel
                {
                    AssetPairId = s.AssetPairId,
                    QuotesSourceType = s.QuotesSourceType,
                    Timestamp = s.Timestamp,
                }).ToList();
        }

        [CanBeNull]
        public AssetPairSettingsModel Get(string assetPairId)
        {
            var entity = GetByKey(GetKeys(assetPairId));
            if (entity == null)
            {
                return null;
            }

            return new AssetPairSettingsModel
            {
                AssetPairId = entity.AssetPairId,
                QuotesSourceType = entity.QuotesSourceType,
                Timestamp = entity.Timestamp,
            };
        }

        public AssetPairQuotesSourceTypeEnum? GetAssetPairQuotesSource(string assetPairId)
        {
            var entity = GetByKey(GetKeys(assetPairId));
            return entity?.QuotesSourceType;
        }
    }
}