using MarginTrading.MarketMaker.Contracts.Enums;

namespace MarginTrading.MarketMaker.Contracts.Messages
{
    public class ExchangeQualityMessage
    {
        public string ExchangeName { get; set; }
        public decimal HedgingPreference { get; set; }
        public ExchangeErrorStateEnum? ErrorState { get; set; }
        public bool OrderbookReceived { get; set; }
    }
}
