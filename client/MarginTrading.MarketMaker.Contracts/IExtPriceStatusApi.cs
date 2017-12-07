using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Models;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface IExtPriceStatusApi
    {
        /// <summary>
        ///     Gets all status
        /// </summary>
        [Get("/api/ExtPriceStatus")]
        IReadOnlyList<ExtPriceStatusModel> List();

        /// <summary>
        ///     Gets status for a single asset pair
        /// </summary>
        [Get("/api/ExtPriceStatus/{assetPairId}")]
        IReadOnlyList<ExtPriceStatusModel> Get(string assetPairId);

        /// <summary>
        ///     Gets logs
        /// </summary>
        [Get("/api/ExtPriceStatus/logs")]
        IReadOnlyList<LogModel> GetLogs();

        /// <summary>
        ///     Gets logs for asset pair
        /// </summary>
        [Get("/api/ExtPriceStatus/logs/{contains}")]
        IReadOnlyList<LogModel> GetLogsFiltered(string contains);
    }
}