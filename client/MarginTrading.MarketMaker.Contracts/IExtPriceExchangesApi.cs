using System.Collections.Generic;
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
        IReadOnlyList<ExchangeExtPriceSettingsModel> List();

        /// <summary>
        ///     Updates an exchange
        /// </summary>
        [Put("/api/ExtPriceExchanges")]
        void Update([Body] ExchangeExtPriceSettingsModel model);

        /// <summary>
        ///     Gets all exchanges for an asset pair
        /// </summary>
        [Get("/api/ExtPriceExchanges/{assetPairId}")]
        IReadOnlyList<ExchangeExtPriceSettingsModel> ListByAssetPair(string assetPairId);

        /// <summary>
        ///     Gets exchange by asset pair and exchange name
        /// </summary>
        [Get("/api/ExtPriceExchanges/{assetPairId}/{exchangeName}")]
        ExchangeExtPriceSettingsModel Get(string assetPairId, string exchangeName);

        /// <summary>
        ///     Adds an exchange with default settings
        /// </summary>
        [Post("/api/ExtPriceExchanges/{assetPairId}")]
        ExchangeExtPriceSettingsModel Add(string assetPairId, string exchangeName);

        /// <summary>
        ///     Deletes an exchange
        /// </summary>
        [Delete("/api/ExtPriceExchanges/{assetPairId}")]
        void Delete(string assetPairId, string exchangeName);
    }
}