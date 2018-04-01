using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IAssetPairsInfoService
    {
        IReadOnlyDictionary<string, AssetPairInfo> Get();
        [CanBeNull] AssetPairInfo Get(string assetPairId);
    }
}