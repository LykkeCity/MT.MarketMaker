using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models
{
    public class CrossRatesSettings
    {
        public string BaseAssetId { get; }
        public ImmutableArray<string> OtherAssetsIds { get; }

        public CrossRatesSettings(string baseAssetId, ImmutableArray<string> otherAssetsIds)
        {
            BaseAssetId = baseAssetId;
            OtherAssetsIds = otherAssetsIds;
        }
    }
}
