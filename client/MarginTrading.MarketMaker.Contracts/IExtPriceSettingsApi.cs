using System.Collections.Generic;
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
        void Update([Body] AssetPairExtPriceSettingsModel setting);

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [Get("/api/ExtPriceSettings")]
        IReadOnlyList<AssetPairExtPriceSettingsModel> List();

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [Put("/api/ExtPriceSettings/{assetPairId}")]
        AssetPairExtPriceSettingsModel Get(string assetPairId);

        /// <summary>
        ///     Gets all hedging preferences
        /// </summary>
        [Get("/api/ExtPriceSettings/hedging-preferences")]
        IReadOnlyList<HedgingPreferenceModel> GetHedgingPreferences();
    }
}