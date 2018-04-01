using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    internal class MarginTradingMarketMakerSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public string MarketMakerId { get; set; }

        [CanBeNull, Optional]
        public string ApplicationInsightsKey { get; set; }
        
        public string LegalEntity { get; set; }
    }
}
