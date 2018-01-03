using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<IReadOnlyList<ExtPriceStatusModel>> List();

        /// <summary>
        ///     Gets status for a single asset pair
        /// </summary>
        [Get("/api/ExtPriceStatus/{assetPairId}")]
        Task<IReadOnlyList<ExtPriceStatusModel>> Get(string assetPairId);

        /// <summary>
        ///     Gets logs
        /// </summary>
        [Get("/api/ExtPriceStatus/logs")]
        Task<IReadOnlyList<TraceModel>> GetLogs();

        /// <summary>
        ///     Gets logs for asset pair
        /// </summary>
        [Get("/api/ExtPriceStatus/logs/{contains}")]
        Task<IReadOnlyList<TraceModel>> GetLogsFiltered(string contains);
    }
}