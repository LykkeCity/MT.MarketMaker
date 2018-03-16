using MarginTrading.MarketMaker.AzureRepositories.StorageModels;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface ISettingsMigrationService
    {
        void Migrate(SettingsRootStorageModel model);
    }
}