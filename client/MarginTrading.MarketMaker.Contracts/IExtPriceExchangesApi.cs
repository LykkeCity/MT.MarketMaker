using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Models;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface IExtPriceExchangesApi
    {
        /// <summary>
        ///     Gets all exchanges
        /// </summary>
        [Get("/api/ExtPriceExchanges")]
        Task<IReadOnlyList<ExchangeExtPriceSettingsModel>> List();

        /// <summary>
        ///     Updates an exchange
        /// </summary>
        [Put("/api/ExtPriceExchanges")]
        Task Update([Body] ExchangeExtPriceSettingsModel model);

        /// <summary>
        ///     Gets all exchanges for an asset pair
        /// </summary>
        [Get("/api/ExtPriceExchanges/{assetPairId}")]
        Task<IReadOnlyList<ExchangeExtPriceSettingsModel>> ListByAssetPair(string assetPairId);

        /// <summary>
        ///     Gets exchange by asset pair and exchange name
        /// </summary>
        [Get("/api/ExtPriceExchanges/{assetPairId}/{exchangeName}")]
        Task<ExchangeExtPriceSettingsModel> Get(string assetPairId, string exchangeName);

        /// <summary>
        ///     Adds an exchange with default settings
        /// </summary>
        [Post("/api/ExtPriceExchanges/{assetPairId}/{exchangeName}")]
        Task<ExchangeExtPriceSettingsModel> Add(string assetPairId, string exchangeName);

        /// <summary>
        ///     Deletes an exchange
        /// </summary>
        [Delete("/api/ExtPriceExchanges/{assetPairId}/{exchangeName}")]
        Task Delete(string assetPairId, string exchangeName);
    }
}