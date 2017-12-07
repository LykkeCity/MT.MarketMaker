using System.Threading.Tasks;
using JetBrains.Annotations;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface ISettingsRootApi
    {
        [Get("/api/SettingsRoot")]
        Task<string> Get();

        [Put("/api/SettingsRoot")]
        Task Set([Body] string settings);
    }
}