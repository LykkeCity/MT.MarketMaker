using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IAssetPairSourceTypeService
    {
        void AddAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);
        void UpdateAssetPairQuotesSourceAsync(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);
        ImmutableDictionary<string, AssetPairQuotesSourceTypeEnum> Get();
        [CanBeNull] AssetPairQuotesSourceTypeEnum? Get(string assetPairId);
    }
}