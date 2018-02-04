using AzureStorage;
using Lykke.SettingsReader;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    public interface IAzureBlobStorageFactoryService
    {
        IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager);
    }
}