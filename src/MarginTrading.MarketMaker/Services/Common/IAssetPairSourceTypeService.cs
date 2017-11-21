using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IAssetPairSourceTypeService
    {
        void AddAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);
        void UpdateAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);
        ImmutableDictionary<string, AssetPairQuotesSourceTypeEnum> Get();
        [CanBeNull] AssetPairQuotesSourceTypeEnum? Get(string assetPairId);
    }
}