using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    internal class AppSettings
    {
        public MarginTradingMarketMakerSettings MarginTradingMarketMaker { get; set; }

        [CanBeNull, Optional]        public SlackNotificationsSettings SlackNotifications { get; set; }        public ExternalServiceClientSettings CandlesHistoryServiceClient { get; set; }    }
}
