using System;
using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class CrossRatesSettings
    {
        public string BaseAssetId { get; }
        public ImmutableArray<string> OtherAssetsIds { get; }

        public CrossRatesSettings(string baseAssetId, ImmutableArray<string> otherAssetsIds)
        {
            BaseAssetId = baseAssetId ?? throw new ArgumentNullException(nameof(baseAssetId));
            OtherAssetsIds = otherAssetsIds;
        }
    }
}
