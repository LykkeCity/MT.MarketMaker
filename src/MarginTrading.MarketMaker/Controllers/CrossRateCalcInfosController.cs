using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class CrossRateCalcInfosController : Controller
    {
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;
        private readonly IAssetPairsSettingsService _assetPairsSettingsService;

        public CrossRateCalcInfosController(ICrossRateCalcInfosService crossRateCalcInfosService, IAssetPairsSettingsService assetPairsSettingsService)
        {
            _crossRateCalcInfosService = crossRateCalcInfosService;
            _assetPairsSettingsService = assetPairsSettingsService;
        }

        /// <summary>
        ///     Replaces all existing settings.
        ///     Warning: changes also AssetPairQuotesSourceType to CrossRates for the specified assets pairs.
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetCrossRateCalcInfos")]
        public async Task<IActionResult> Set([FromBody] IReadOnlyList<CrossRateCalcInfoModel> settings)
        {
            _crossRateCalcInfosService.Set(settings.Select(Convert).ToList());
            foreach (var assetPairId in settings.Select(s => s.ResultingPairId))
            {
                await _assetPairsSettingsService.SetAssetPairQuotesSourceAsync(assetPairId, AssetPairQuotesSourceTypeEnum.CrossRates);
            }

            return Ok(new {success = true });
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