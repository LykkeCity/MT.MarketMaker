namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class HedgingSettingsModel
    {
        public decimal DefaultPreference { get; set; }
        public bool IsTemporarilyUnavailable { get; set; }
    }
}