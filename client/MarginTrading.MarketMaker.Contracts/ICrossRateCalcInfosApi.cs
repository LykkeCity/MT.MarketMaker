using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Models;
using Refit;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface ICrossRateCalcInfosApi
    {
        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [Get("/api/CrossRateCalcInfos")]
        Task<IReadOnlyList<CrossRateCalcInfoModel>> List();

        /// <summary>
        ///     Update setting for a resulting cross-pair
        /// </summary>
        [Put("/api/CrossRateCalcInfos")]
        Task Update([Body] CrossRateCalcInfoModel model);

        /// <summary>
        ///     Gets cross-pair by asset pair
        /// </summary>
        [Get("/api/CrossRateCalcInfos/{assetPairId}")]
        Task<CrossRateCalcInfoModel> Get(string assetPairId);
    }
}
