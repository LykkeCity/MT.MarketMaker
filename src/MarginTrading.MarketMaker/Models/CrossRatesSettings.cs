using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models
{
    public class CrossRatesSettings
    {
        public ImmutableArray<string> BaseAssetsIds { get; }
        public ImmutableArray<string> OtherAssetsIds { get; }

        public CrossRatesSettings(ImmutableArray<string> baseAssetsIds, ImmutableArray<string> otherAssetsIds)
        {
            BaseAssetsIds = baseAssetsIds;
            OtherAssetsIds = otherAssetsIds;
        }
    }
}
