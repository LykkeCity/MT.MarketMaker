using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories.Entities;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Api;
using Rocks.Caching;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class AssetPairsSettingsService : CachedEntityAccessorService<AssetPairSettingsEntity>,
        IAssetPairsSettingsService
    {
        private readonly IAssetsPairsSettingsRepository _assetsPairsSettingsRepository;

        public AssetPairsSettingsService(ICacheProvider cache,
            IAssetsPairsSettingsRepository assetsPairsSettingsRepository)
            : base(cache, assetsPairsSettingsRepository)
        {
            _assetsPairsSettingsRepository = assetsPairsSettingsRepository;
        }

        public Task SetAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum assetPairQuotesSourceType)
        {
            return UpdateByKeyAsync(GetKeys(assetPairId), e => e.QuotesSourceType = assetPairQuotesSourceType);
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

        public async Task DeleteAsync(string assetPairId)
        {
            var keys = GetKeys(assetPairId);
            await _assetsPairsSettingsRepository.DeleteIfExistAsync(CachedEntityAccessorService<AssetPairSettingsEntity>.GenerateEntityFromKeys(keys));
            DeleteByKey(keys);
        }

        public AssetPairQuotesSourceTypeEnum? GetAssetPairQuotesSource(string assetPairId)
        {
            var entity = GetByKey(GetKeys(assetPairId));
            return entity?.QuotesSourceType;
        }

        private static CachedEntityAccessorService.EntityKeys GetKeys(string assetPairId)
        {
            return new CachedEntityAccessorService.EntityKeys(AssetPairSettingsEntity.GeneratePartitionKey(), AssetPairSettingsEntity
                .GenerateRowKey(assetPairId));
        }
    }
}