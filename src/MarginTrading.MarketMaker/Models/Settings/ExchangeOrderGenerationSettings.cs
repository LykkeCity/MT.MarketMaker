using System;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class ExchangeOrderGenerationSettings
    {
        public decimal VolumeMultiplier { get; }
        public TimeSpan OrderRenewalDelay { get; }

        public ExchangeOrderGenerationSettings(decimal volumeMultiplier, TimeSpan orderRenewalDelay)
        {
            VolumeMultiplier = volumeMultiplier;
            OrderRenewalDelay = orderRenewalDelay;
        }
    }
}