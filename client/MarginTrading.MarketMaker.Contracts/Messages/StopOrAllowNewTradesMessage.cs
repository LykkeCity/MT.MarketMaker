namespace MarginTrading.MarketMaker.Contracts.Messages
{
    public class StopOrAllowNewTradesMessage
    {
        public string AssetPairId { get; set; }
        public string MarketMakerId { get; set; }
        public string Reason { get; set; }
        public bool Stop { get; set; }
    }
}
