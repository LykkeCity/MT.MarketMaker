using System;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models
{
    public class ExchangeQuality
    {
        public string ExchangeName { get; }
        public decimal HedgingPreference { get; }
        public ExchangeErrorState? Error { get; }
        public bool OrderbookReceived { get; }
        public DateTime? LastOrderbookReceivedTime { get; }

        public ExchangeQuality(string exchangeName, decimal hedgingPreference, ExchangeErrorState? error, bool orderbookReceived, DateTime? lastOrderbookReceivedTime)
        {
            ExchangeName = exchangeName;
            HedgingPreference = hedgingPreference;
            Error = error;
            OrderbookReceived = orderbookReceived;
            LastOrderbookReceivedTime = lastOrderbookReceivedTime;
        }

        public override string ToString()
        {
            return $"{ExchangeName} ({Error?.ToString() ?? "NoOrderbook"}, {HedgingPreference:P2})";
        }
    }
}
