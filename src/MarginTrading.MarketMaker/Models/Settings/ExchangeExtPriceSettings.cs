using System;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class ExchangeExtPriceSettings
    {
        public TimeSpan OrderbookOutdatingThreshold { get; }
        public ExchangeDisabledSettings Disabled { get; }
        public ExchangeHedgingSettings Hedging { get; }
        public ExchangeOrderGenerationSettings OrderGeneration { get; }

        public ExchangeExtPriceSettings(TimeSpan orderbookOutdatingThreshold, ExchangeDisabledSettings disabled,
            ExchangeHedgingSettings hedging, ExchangeOrderGenerationSettings orderGeneration)
        {
            OrderbookOutdatingThreshold = orderbookOutdatingThreshold;
            Disabled = disabled;
            Hedging = hedging;
            OrderGeneration = orderGeneration;
        }

        public static ExchangeExtPriceSettings Change(ExchangeExtPriceSettings src, ExchangeDisabledSettings disabled)
        {
            return new ExchangeExtPriceSettings(src.OrderbookOutdatingThreshold, disabled, src.Hedging,
                src.OrderGeneration);
        }
    }
}