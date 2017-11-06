using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class CrossRatesSettingsModel
    {
        public ImmutableArray<string> BaseAssetsIds { get; set; }
        public ImmutableArray<string> OtherAssetsIds { get; set; }
    }
}
