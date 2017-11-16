using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class CrossRatesSettingsModel
    {
        public string BaseAssetId { get; set; }
        public ImmutableArray<string> OtherAssetsIds { get; set; }
    }
}
