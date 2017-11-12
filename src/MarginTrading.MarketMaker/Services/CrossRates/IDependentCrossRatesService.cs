using System.Collections.Generic;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface IDependentCrossRatesService
    {
        IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs(string assetPairId);
    }
}