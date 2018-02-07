using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface ISettingsStorageService
    {
        [CanBeNull] SettingsRoot Read();
        void Write(SettingsRoot root);
    }
}
