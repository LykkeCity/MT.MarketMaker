using System;

namespace MarginTrading.MarketMaker.Models.Settings
{
    internal class ExchangeExtPriceSettings
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
            public double DefaultPreference { get; }
            public bool IsTemporarilyUnavailable { get; }

            public HedgingSettings(double defaultPreference, bool isTemporarilyUnavailable)
            {
                DefaultPreference = defaultPreference;
                IsTemporarilyUnavailable = isTemporarilyUnavailable;
            }
        }

        public class OrderGenerationSettings
        {
            public double VolumeMultiplier { get; }
            public TimeSpan OrderRenewalDelay { get; }

            public OrderGenerationSettings(double volumeMultiplier, TimeSpan orderRenewalDelay)
            {
                VolumeMultiplier = volumeMultiplier;
                OrderRenewalDelay = orderRenewalDelay;
            }
        }
    }
}