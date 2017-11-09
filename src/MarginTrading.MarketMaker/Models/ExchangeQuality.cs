using System;
using MarginTrading.MarketMaker.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Models
{
    public class ExchangeQuality
    {
        public string Exchange { get; }
        public decimal HedgingPreference { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeErrorState? Error { get; }
        public bool OrderbookReceived { get; }
        public DateTime? LastOrderbookReceivedTime { get; }

        public ExchangeQuality(string exchange, decimal hedgingPreference, ExchangeErrorState? error, bool orderbookReceived, DateTime? lastOrderbookReceivedTime)
        {
            Exchange = exchange;
            HedgingPreference = hedgingPreference;
            Error = error;
            OrderbookReceived = orderbookReceived;
            LastOrderbookReceivedTime = lastOrderbookReceivedTime;
        }

        public override string ToString()
        {
            return $"{Exchange} ({Error?.ToString() ?? "NoOrderbook"}, {HedgingPreference:P2})";
        }
    }
}
