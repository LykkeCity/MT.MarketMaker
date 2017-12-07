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
        [Headers("Content-Type: application/json; charset=UTF-8")]
        Task Set([Body] string settings);
    }
}