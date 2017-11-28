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
            Disabled = disabled ?? throw new ArgumentNullException(nameof(disabled));
            Hedging = hedging ?? throw new ArgumentNullException(nameof(hedging));
            OrderGeneration = orderGeneration ?? throw new ArgumentNullException(nameof(orderGeneration));
        }
        
        public static ExchangeExtPriceSettings Change(ExchangeExtPriceSettings src, ExchangeDisabledSettings disabled)
        {
            return new ExchangeExtPriceSettings(src.OrderbookOutdatingThreshold, disabled, src.Hedging, src.OrderGeneration);
        }
    }
}