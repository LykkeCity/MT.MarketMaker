using JetBrains.Annotations;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface ISettingsRootApi
    {
        [Get("/api/SettingsRoot")]
        string Get();

        [Put("/api/SettingsRoot")]
        void Set([Body] string settings);
    }
}