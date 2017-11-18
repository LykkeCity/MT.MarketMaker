using System;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class ExchangeExtPriceSettingsStorageModel
    {
        public TimeSpan OrderbookOutdatingThreshold { get; set; }
        public DisabledSettings Disabled { get; set; } = new DisabledSettings();
        public HedgingSettings Hedging { get; set; } = new HedgingSettings();
        public OrderGenerationSettings OrderGeneration { get; set; } = new OrderGenerationSettings();

        public class DisabledSettings
        {
            public bool IsTemporarilyDisabled { get; set; }
            public string Reason { get; set; }
        }

        public class HedgingSettings
        {
            public double DefaultPreference { get; set; }
            public bool IsTemporarilyUnavailable { get; set; }
        }

        public class OrderGenerationSettings
        {
            public double VolumeMultiplier { get; set; }
            public TimeSpan OrderRenewalDelay { get; set; }
        }
    }
}