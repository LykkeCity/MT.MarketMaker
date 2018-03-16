using System.Text;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    public class AzureBlobJsonStorage : IAzureBlobJsonStorage
    {
        private readonly IBlobStorage _blobStorage;

        public AzureBlobJsonStorage(IReloadingManager<string> connectionStringManager)
        {
            _blobStorage = AzureBlobStorage.Create(connectionStringManager);
        }

        public T Read<T>(string container, string key) where T: class 
        {
            if (_blobStorage.HasBlobAsync(container, key).Result)
            {
                var data = _blobStorage.GetAsync(container, key).GetAwaiter().GetResult().ToBytes();
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
            }
            
            return null;
        }

        public void Write<T>(string container, string key, T obj) where T: class 
        {
            var data = JsonConvert.SerializeObject(obj).ToUtf8Bytes();
            _blobStorage.SaveBlobAsync(container, key, data).GetAwaiter().GetResult();
        }
    }
}