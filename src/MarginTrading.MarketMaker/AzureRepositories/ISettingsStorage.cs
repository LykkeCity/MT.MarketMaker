using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface ISettingsStorage
    {
        [CanBeNull] SettingsRoot Read();
        void Write(SettingsRoot model);
    }
}
