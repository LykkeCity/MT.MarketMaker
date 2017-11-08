using System;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class ExtPriceStatusModel
    {
        public string Exchange { get; set; }
        public BestPricesModel BestPrices { get; set; }
        public decimal HedgingPreference { get; set; }
        public bool OrderbookReceived { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
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

        [CanBeNull]
        public static ExchangeErrorStateModel? ConvertErrorStateModel(ExchangeErrorState? errorState)
        {
            return (ExchangeErrorStateModel?) errorState;
        }
    }
}
