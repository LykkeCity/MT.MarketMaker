using System;

namespace MarginTrading.MarketMaker.Services.CrossRates.Models
{
    internal class CrossRateCalcInfo
    {
        public string ResultingPairId { get; }
        public CrossRateSourceAssetPair Source1 { get; }
        public CrossRateSourceAssetPair Source2 { get; }

        public CrossRateCalcInfo(string resultingPairId, CrossRateSourceAssetPair source1, CrossRateSourceAssetPair source2)
        {
            ResultingPairId = resultingPairId ?? throw new ArgumentNullException(nameof(resultingPairId));
            Source1 = source1 ?? throw new ArgumentNullException(nameof(source1));
            Source2 = source2 ?? throw new ArgumentNullException(nameof(source2));
        }
    }
}