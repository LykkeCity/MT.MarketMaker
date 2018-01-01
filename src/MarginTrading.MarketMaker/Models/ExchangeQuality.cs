using System;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models
{
    public class ExchangeQuality
    {
        public string ExchangeName { get; }
        public decimal HedgingPreference { get; }
        public ExchangeErrorStateEnum? ErrorState { get; }
        public bool OrderbookReceived { get; }
        public DateTime? LastOrderbookReceivedTime { get; }

        public ExchangeQuality(string exchangeName, decimal hedgingPreference, ExchangeErrorStateEnum? errorState, bool orderbookReceived, DateTime? lastOrderbookReceivedTime)
        {
            ExchangeName = exchangeName;
            HedgingPreference = hedgingPreference;
            ErrorState = errorState;
            OrderbookReceived = orderbookReceived;
            LastOrderbookReceivedTime = lastOrderbookReceivedTime;
        }

        public override string ToString()
        {
            return $"{ExchangeName} ({(ErrorState == null ? "NoOrderbook" : ErrorState.ToString())}, {HedgingPreference:P2})";
        }
    }
}
