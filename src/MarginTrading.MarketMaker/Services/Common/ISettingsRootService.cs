using System.Threading.Tasks;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common
{
    internal interface ISettingsRootService
    {
        Task Set(SettingsRoot settings);
        SettingsRoot Get();
    }
}