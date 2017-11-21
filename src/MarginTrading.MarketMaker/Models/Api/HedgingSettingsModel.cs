namespace MarginTrading.MarketMaker.Models.Api
{
    public class HedgingSettingsModel
    {
        public decimal DefaultPreference { get; set; }
        public bool IsTemporarilyUnavailable { get; set; }
    }
}