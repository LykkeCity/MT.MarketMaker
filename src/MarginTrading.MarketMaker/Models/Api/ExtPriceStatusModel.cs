using System;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class ExtPriceStatusModel
    {
        public string ExchangeName { get; set; }
        public BestPricesModel BestPrices { get; set; }
        public decimal HedgingPreference { get; set; }
        public bool OrderbookReceived { get; set; }
        public ExchangeErrorStateModel? Error { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? LastOrderbookReceivedTime { get; set; }

        public enum ExchangeErrorStateModel
        {
            None = 0,
            Outlier = 1,
            Outdated = 2,
            Disabled = 4
        }
    }
}
