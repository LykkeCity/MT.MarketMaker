﻿using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Messages
{
    public class ExchangeQualityMessage
    {
        public string Exchange { get; set; }
        public decimal HedgingPreference { get; set; }
        public ExchangeErrorState? Error { get; set; }
        public bool OrderbookReceived { get; set; }
    }
}
