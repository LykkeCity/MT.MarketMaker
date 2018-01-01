using System.Text;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MarginTrading.MarketMaker.Settings;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsStorageService : ISettingsStorageService
    {
        private readonly IConvertService _convertService;
        private readonly IBlobStorage _blobStorage;
        private const string BlobContainer = "MtMmSettings";
        private const string Key = "SettingsRoot";

        public SettingsStorageService(IReloadingManager<MarginTradingMarketMakerSettings> settings, IConvertService convertService)
        {
            _convertService = convertService;
            _blobStorage = AzureBlobStorage.Create(settings.Nested(s => s.Db.ConnectionString));
        }

        public SettingsRoot Read()
        {
            if (_blobStorage.HasBlobAsync(BlobContainer, Key).Result)
            {
                var data = _blobStorage.GetAsync(BlobContainer, Key).GetAwaiter().GetResult().ToBytes();
                var str = Encoding.UTF8.GetString(data);
                return Convert(JsonConvert.DeserializeObject<SettingsRootStorageModel>(str));
            }

            return null;
        }

        private SettingsRoot Convert(SettingsRootStorageModel model)
        {
            return _convertService.Convert<SettingsRootStorageModel, SettingsRoot>(model);
        }

        private SettingsRootStorageModel Convert(SettingsRoot root)
        {
            return _convertService.Convert<SettingsRoot, SettingsRootStorageModel>(root);
        }

        public void Write(SettingsRoot model)
        {
            var data = JsonConvert.SerializeObject(Convert(model)).ToUtf8Bytes();
            _blobStorage.SaveBlobAsync(BlobContainer, Key, data).GetAwaiter().GetResult();
        }
    }
}