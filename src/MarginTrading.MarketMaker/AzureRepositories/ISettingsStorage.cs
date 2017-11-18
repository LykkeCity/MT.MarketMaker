using System.Threading.Tasks;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface ISettingsStorage
    {
        Task<SettingsRoot> Get();
        Task Set(SettingsRoot model);
    }
}
