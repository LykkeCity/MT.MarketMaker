using System;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class ExtPriceStatusModel
    {
        public string AssetPairId { get; set; }
        public string ExchangeName { get; set; }
        public BestPricesModel BestPrices { get; set; }
        public decimal HedgingPreference { get; set; }
        public bool OrderbookReceived { get; set; }
        public string ErrorState { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? LastOrderbookReceivedTime { get; set; }
    }
}
