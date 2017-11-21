using System;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class ExchangeExtPriceSettings
    {
        public TimeSpan OrderbookOutdatingThreshold { get; }
        public DisabledSettings Disabled { get; }
        public HedgingSettings Hedging { get; }
        public OrderGenerationSettings OrderGeneration { get; }

        public ExchangeExtPriceSettings(TimeSpan orderbookOutdatingThreshold, DisabledSettings disabled,
            HedgingSettings hedging, OrderGenerationSettings orderGeneration)
        {
            OrderbookOutdatingThreshold = orderbookOutdatingThreshold;
            Disabled = disabled ?? throw new ArgumentNullException(nameof(disabled));
            Hedging = hedging ?? throw new ArgumentNullException(nameof(hedging));
            OrderGeneration = orderGeneration ?? throw new ArgumentNullException(nameof(orderGeneration));
        }

        public class DisabledSettings
        {
            public DisabledSettings(bool isTemporarilyDisabled, string reason)
            {
                IsTemporarilyDisabled = isTemporarilyDisabled;
                Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            }

            public bool IsTemporarilyDisabled { get; }
            public string Reason { get; }
        }

        public class HedgingSettings
        {
            public decimal DefaultPreference { get; }
            public bool IsTemporarilyUnavailable { get; }

            public HedgingSettings(decimal defaultPreference, bool isTemporarilyUnavailable)
            {
                DefaultPreference = defaultPreference;
                IsTemporarilyUnavailable = isTemporarilyUnavailable;
            }
        }

        public class OrderGenerationSettings
        {
            public decimal VolumeMultiplier { get; }
            public TimeSpan OrderRenewalDelay { get; }

            public OrderGenerationSettings(decimal volumeMultiplier, TimeSpan orderRenewalDelay)
            {
                VolumeMultiplier = volumeMultiplier;
                OrderRenewalDelay = orderRenewalDelay;
            }
        }
    }
}