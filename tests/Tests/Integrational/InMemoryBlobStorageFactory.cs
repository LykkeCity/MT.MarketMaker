using System.Collections.Concurrent;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories;

namespace Tests.Integrational
{
    internal class InMemoryBlobStorageFactory : IAzureBlobStorageFactoryService
    {
        private readonly ConcurrentDictionary<string, IAzureBlobJsonStorage> _blobs =
            new ConcurrentDictionary<string, IAzureBlobJsonStorage>();

        public IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager)
        {
            return Create(connectionStringManager.CurrentValue);
        }
        
        public IAzureBlobJsonStorage Create(string connectionString)
        {
            return _blobs.GetOrAdd(connectionString, k => new AzureBlobJsonStorageInMemory());
        }
    }
}