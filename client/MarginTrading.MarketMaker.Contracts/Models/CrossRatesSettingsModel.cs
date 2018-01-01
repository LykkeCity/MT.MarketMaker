using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class CrossRatesSettingsModel
    {
        public string BaseAssetId { get; set; }
        public ImmutableArray<string> OtherAssetsIds { get; set; }
    }
}