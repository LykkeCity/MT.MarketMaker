using System;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class OrderGenerationSettingsModel
    {
        public decimal VolumeMultiplier { get; set; }
        public TimeSpan OrderRenewalDelay { get; set; }
    }
}