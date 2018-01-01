using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface ICrossRateCalcInfosService
    {
        void Update(CrossRateCalcInfo info);
        ImmutableDictionary<string, CrossRateCalcInfo> Get();
        IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId);
        CrossRateCalcInfo GetDefault(string assetPairId);
        [CanBeNull] CrossRateCalcInfo Get(string assetPairId);
    }
}
