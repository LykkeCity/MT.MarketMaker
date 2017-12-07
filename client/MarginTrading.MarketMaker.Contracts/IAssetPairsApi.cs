using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface IAssetPairsApi
    {
        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [Get("/api/AssetPairs")]
        IReadOnlyList<AssetPairModel> List();

        /// <summary>
        ///     Adds default settings for an asset pair
        /// </summary>
        [Post("/api/AssetPairs/{assetPairId}")]
        void Add(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [Get("/api/AssetPairs/{assetPairId}")]
        AssetPairModel Get(string assetPairId);

        /// <summary>
        ///     Updates settings for an asset pair
        /// </summary>
        [Put("/api/AssetPairs")]
        void Update([Body] AssetPairInputModel model);

        /// <summary>
        ///     Deletes settings for an asset pair
        /// </summary>
        [Delete("/api/AssetPairs/{assetPairId}")]
        void Delete(string assetPairId);
    }
}