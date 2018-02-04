using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsStorageService : ISettingsStorageService
    {
        private readonly IConvertService _convertService;
        private readonly ISettingsMigrationService _settingsMigrationService;
        private readonly IAzureBlobJsonStorage _blobStorage;
        private const string BlobContainer = "MtMmSettings";
        private const string Key = "SettingsRoot";
        internal const int CurrentStorageModelVersion = 2;

        public SettingsStorageService(IReloadingManager<MarginTradingMarketMakerSettings> settings,
            IConvertService convertService, ISettingsMigrationService settingsMigrationService,
            IAzureBlobStorageFactoryService azureBlobStorageFactoryService)
        {
            _convertService = convertService;
            _settingsMigrationService = settingsMigrationService;
            _blobStorage = azureBlobStorageFactoryService.Create(settings.Nested(s => s.Db.ConnectionString));
        }

        public SettingsRoot Read()
        {
            var settingsRootStorageModel = _blobStorage.Read<SettingsRootStorageModel>(BlobContainer, Key);
            if (settingsRootStorageModel == null)
                return null;

            _settingsMigrationService.Migrate(settingsRootStorageModel);
            return Convert(settingsRootStorageModel);
        }

        private SettingsRoot Convert(SettingsRootStorageModel model)
        {
            return _convertService.Convert<SettingsRootStorageModel, SettingsRoot>(model);
        }

        private SettingsRootStorageModel Convert(SettingsRoot root)
        {
            return _convertService.Convert<SettingsRoot, SettingsRootStorageModel>(root,
                o => o.ConfigureMap().ForMember(m => m.Version, c => c.Ignore()));
        }

        public void Write(SettingsRoot root)
        {
            var storageModel = Convert(root);
            storageModel.Version = CurrentStorageModelVersion;
            _blobStorage.Write(BlobContainer, Key, storageModel);
        }
    }
}