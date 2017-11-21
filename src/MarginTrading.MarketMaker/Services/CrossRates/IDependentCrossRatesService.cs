using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface IDependentCrossRatesService
    {
        IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId);
        IEnumerable<string> GetExistingCrossPairs();
        [CanBeNull] CrossRateCalcInfo GetForResultingPairId(string assetPairId);
    }
}