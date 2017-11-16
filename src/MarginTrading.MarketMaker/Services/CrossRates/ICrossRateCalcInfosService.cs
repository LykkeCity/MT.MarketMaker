using System.Collections.Generic;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface ICrossRateCalcInfosService
    {
        void Set(IEnumerable<CrossRateCalcInfo> models);
        IReadOnlyList<CrossRateCalcInfo> Get();
        IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId);
    }
}
