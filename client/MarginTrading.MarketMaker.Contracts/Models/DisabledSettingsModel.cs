namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class DisabledSettingsModel
    {
        public bool IsTemporarilyDisabled { get; set; }
        public string Reason { get; set; }
    }
}