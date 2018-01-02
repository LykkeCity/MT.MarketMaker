using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IAssetPairSourceTypeService
    {
        void AddAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeDomainEnum sourceType);
        void UpdateAssetPairQuotesSource(string assetPairId, AssetPairQuotesSourceTypeDomainEnum sourceType);
        ImmutableDictionary<string, AssetPairQuotesSourceTypeDomainEnum> Get();
        [CanBeNull] AssetPairQuotesSourceTypeDomainEnum? Get(string assetPairId);
        void Delete(string assetPairId);
    }
}