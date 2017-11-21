namespace MarginTrading.MarketMaker.Models.Settings
{
    public class ExchangeHedgingSettings
    {
        public decimal DefaultPreference { get; }
        public bool IsTemporarilyUnavailable { get; }

        public ExchangeHedgingSettings(decimal defaultPreference, bool isTemporarilyUnavailable)
        {
            DefaultPreference = defaultPreference;
            IsTemporarilyUnavailable = isTemporarilyUnavailable;
        }
    }
}