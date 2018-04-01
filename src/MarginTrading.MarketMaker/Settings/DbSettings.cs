using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string ConnectionString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureBlobCheck]
        public string QueuePersistanceRepositoryConnString { get; set; }
    }
}
