using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Models;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface IExtPriceSettingsApi
    {
        /// <summary>
        ///     Updates settings for an asset pair
        /// </summary>
        [Put("/api/ExtPriceSettings")]
        Task Update([Body] AssetPairExtPriceSettingsModel setting);

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [Get("/api/ExtPriceSettings")]
        Task<IReadOnlyList<AssetPairExtPriceSettingsModel>> List();

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [Get("/api/ExtPriceSettings/{assetPairId}")]
        Task<AssetPairExtPriceSettingsModel> Get(string assetPairId);

        /// <summary>
        ///     Gets all hedging preferences
        /// </summary>
        [Get("/api/ExtPriceSettings/hedging-preferences")]
        Task<IReadOnlyList<HedgingPreferenceModel>> GetHedgingPreferences();
    }
}