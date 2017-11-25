using System.Text;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Settings;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsStorageService : ISettingsStorageService
    {
        private readonly IBlobStorage _blobStorage;
        private const string BlobContainer = "MtMmSettings";
        private const string Key = "SettingsRoot";

        public SettingsStorageService(IReloadingManager<MarginTradingMarketMakerSettings> settings)
        {
            _blobStorage = AzureBlobStorage.Create(settings.Nested(s => s.Db.ConnectionString));
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