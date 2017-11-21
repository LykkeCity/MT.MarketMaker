using System;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class ExchangeExtPriceSettingsModel
    {
        public string ExchangeName { get; set; }
        public TimeSpan OrderbookOutdatingThreshold { get; set; }
        public DisabledSettingsModel Disabled { get; set; } = new DisabledSettingsModel();
        public HedgingSettingsModel Hedging { get; set; } = new HedgingSettingsModel();
        public OrderGenerationSettingsModel OrderGeneration { get; set; } = new OrderGenerationSettingsModel();
    }
}
