using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.CrossRates;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class CrossRatesSettingsController : Controller
    {
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;
        private readonly IAssetPairsSettingsService _assetPairsSettingsService;
        private readonly IDependentCrossRatesService _dependentCrossRatesService;

        public CrossRatesSettingsController(
            ICrossRatesSettingsService crossRatesSettingsService,
            IAssetPairsSettingsService assetPairsSettingsService,
            IDependentCrossRatesService dependentCrossRatesService)
        {
            _crossRatesSettingsService = crossRatesSettingsService;
            _assetPairsSettingsService = assetPairsSettingsService;
            _dependentCrossRatesService = dependentCrossRatesService;
        }

        /// <summary>
        ///     Replaces all existing settings.
        ///     Warning: changes also AssetPairQuotesSourceType to CrossRates for the specified assets pairs.
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetCrossRatesSettings")]
        public async Task<IActionResult> Set([FromBody] IEnumerable<CrossRatesSettingsModel> settings)
        {
            _crossRatesSettingsService.Set(settings.Select(Convert).ToList());
            var existingCrossPairs = _dependentCrossRatesService.GetExistingCrossPairs().ToList();
            foreach (var assetPairId in existingCrossPairs)
            {
                await _assetPairsSettingsService.SetAssetPairQuotesSourceAsync(assetPairId, AssetPairQuotesSourceTypeEnum.CrossRates);
            }

            return Ok(new {success = true, existingCrossPairs });
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetCrossRatesSettings")]
        public IEnumerable<CrossRatesSettingsModel> Get()
        {
            return _crossRatesSettingsService.Get().Select(Convert);
        }

        private static CrossRatesSettingsModel Convert(CrossRatesSettings settings)
        {
            return new CrossRatesSettingsModel
            {
                BaseAssetId = settings.BaseAssetId,
                OtherAssetsIds = settings.OtherAssetsIds,
            };
        }

        private static CrossRatesSettings Convert(CrossRatesSettingsModel model)
        {
            return new CrossRatesSettings(model.BaseAssetId, model.OtherAssetsIds);
        }
    }
}