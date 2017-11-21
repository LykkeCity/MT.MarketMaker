using System.Collections.Generic;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface ICrossRateCalcInfosService
    {
        void Update(CrossRateCalcInfo info);
        IReadOnlyList<CrossRateCalcInfo> Get();
        IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId);
        CrossRateCalcInfo GetDefault();
        CrossRateCalcInfo Get(string assetPairId);
        void Add(CrossRateCalcInfo info);
    }
}
