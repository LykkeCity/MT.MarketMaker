using System.Text;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Models.Settings;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsStorage : ISettingsStorage
    {
        private readonly IBlobStorage _blobStorage;
        private const string BlobContainer = "MtMmSettings";
        private const string Key = "SettingsRoot";

        public SettingsStorage(IReloadingManager<string> connectionString)
        {
            _blobStorage = AzureBlobStorage.Create(connectionString);
        }

        public SettingsRoot Read()
        {
            if (_blobStorage.HasBlobAsync(BlobContainer, Key).Result)
            {
                var data = _blobStorage.GetAsync(BlobContainer, Key).GetAwaiter().GetResult().ToBytes();
                var str = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<SettingsRoot>(str);
            }

            return null;
        }

        public void Write(SettingsRoot model)
        {
            var data = JsonConvert.SerializeObject(model).ToUtf8Bytes();
            _blobStorage.SaveBlobAsync(BlobContainer, Key, data).GetAwaiter().GetResult();
        }
    }
}