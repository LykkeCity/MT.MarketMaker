namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class HedgingPreferenceModel
    {
        public string AssetPairId { get; set; }
        public string Exchange { get; set; }
        public decimal Preference { get; set; }
        public bool IsHedgingUnavailable { get; set; }
        public bool IsPrimary { get; set; }
        public string ErrorState { get; set; }
    }
}