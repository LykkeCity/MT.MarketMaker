using Lykke.SettingsReader;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    public class AzureBlobStorageFactoryService : IAzureBlobStorageFactoryService
    {
        public IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager)
        {
            return new AzureBlobJsonStorage(connectionStringManager);
        }
    }
}