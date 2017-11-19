using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    [UsedImplicitly]
    internal class AssetPairSourceTypeService : IAssetPairSourceTypeService
    {
        private readonly ISettingsRootService _settingsRootService;

        public AssetPairSourceTypeService(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
        }

        public Task AddAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            return _settingsRootService.Set(assetPairId,
                old =>
                {
                    old.RequiredEqualsTo(null, nameof(old));
                    return new AssetPairSettings(sourceType, null, null);
                });
        }

        public Task UpdateAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            return _settingsRootService.Set(assetPairId,
                old =>
                {
                    old.RequiredNotNull(nameof(old));
                    return new AssetPairSettings(sourceType, old.ExtPriceSettings,
                        old.CrossRateCalcInfo);
                });
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
    }
}