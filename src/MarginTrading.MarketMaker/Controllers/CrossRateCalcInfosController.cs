using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class CrossRateCalcInfosController : Controller
    {
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;

        public CrossRateCalcInfosController(ICrossRateCalcInfosService crossRateCalcInfosService, IAssetPairSourceTypeService assetPairSourceTypeService)
        {
            _crossRateCalcInfosService = crossRateCalcInfosService;
            _assetPairSourceTypeService = assetPairSourceTypeService;
        }

        /// <summary>
        ///     Update setting for a resulting cross-pair
        /// </summary>
        [HttpPost]
        public IActionResult Update([FromBody] CrossRateCalcInfoModel settings)
        {
            _crossRateCalcInfosService.Update(Convert(settings));
            return Ok(new { success = true });
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public IEnumerable<CrossRateCalcInfoModel> List()
        {
            return _crossRateCalcInfosService.Get().Values.Select(Convert);
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public CrossRateCalcInfoModel Get(string assetPairId)
        {
            return Convert(_crossRateCalcInfosService.Get(assetPairId));
        }

        private static CrossRateCalcInfoModel Convert(CrossRateCalcInfo settings)
        {
            return new CrossRateCalcInfoModel
            {
                ResultingPairId = settings.ResultingPairId,
                Source1 = new CrossRateSourceAssetPairModel
                {
                    Id = settings.Source1.Id,
                    IsTransitoryAssetQuoting = settings.Source1.IsTransitoryAssetQuoting,
                },
                Source2 = new CrossRateSourceAssetPairModel
                {
                    Id = settings.Source2.Id,
                    IsTransitoryAssetQuoting = settings.Source2.IsTransitoryAssetQuoting,
                }
            };
        }

        private static CrossRateCalcInfo Convert(CrossRateCalcInfoModel model)
        {
            return new CrossRateCalcInfo(model.ResultingPairId,
                new CrossRateSourceAssetPair(model.Source1.Id, model.Source1.IsTransitoryAssetQuoting),
                new CrossRateSourceAssetPair(model.Source2.Id, model.Source2.IsTransitoryAssetQuoting));
        }
    }
}