using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<IReadOnlyList<AssetPairModel>> List();

        /// <summary>
        ///     Adds default settings for an asset pair
        /// </summary>
        [Post("/api/AssetPairs/{assetPairId}")]
        Task Add(string assetPairId, AssetPairQuotesSourceTypeEnum sourceType);

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [Get("/api/AssetPairs/{assetPairId}")]
        Task<AssetPairModel> Get(string assetPairId);

        /// <summary>
        ///     Updates settings for an asset pair
        /// </summary>
        [Put("/api/AssetPairs")]
        Task Update([Body] AssetPairInputModel model);

        /// <summary>
        ///     Deletes settings for an asset pair
        /// </summary>
        [Delete("/api/AssetPairs/{assetPairId}")]
        Task Delete(string assetPairId);
    }
}