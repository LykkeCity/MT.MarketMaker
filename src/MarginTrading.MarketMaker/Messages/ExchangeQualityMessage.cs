using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Messages
{
    public class ExchangeQualityMessage
    {
        public string ExchangeName { get; set; }
        public decimal HedgingPreference { get; set; }
        public ExchangeErrorStateEnum? ErrorState { get; set; }
        public bool OrderbookReceived { get; set; }
    }
}
