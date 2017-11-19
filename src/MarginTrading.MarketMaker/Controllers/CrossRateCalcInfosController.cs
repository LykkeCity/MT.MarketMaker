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
        ///     Replaces all existing settings.
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetCrossRateCalcInfos")]
        public IActionResult Set([FromBody] IReadOnlyList<CrossRateCalcInfoModel> settings)
        {
            _crossRateCalcInfosService.Set(settings.Select(Convert).ToList());
            return Ok(new { success = true });
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetCrossRateCalcInfos")]
        public IEnumerable<CrossRateCalcInfoModel> Get()
        {
            return _crossRateCalcInfosService.Get().Select(Convert);
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