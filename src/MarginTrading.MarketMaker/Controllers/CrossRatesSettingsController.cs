using System.Collections.Generic;
using System.Linq;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Controllers
{
    //note: hidden for now
    [Route("api/[controller]")]
    internal class CrossRatesSettingsController : Controller
    {
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;
        private readonly IDependentCrossRatesService _dependentCrossRatesService;

        public CrossRatesSettingsController(
            ICrossRatesSettingsService crossRatesSettingsService,
            IAssetPairSourceTypeService assetPairSourceTypeService,
            IDependentCrossRatesService dependentCrossRatesService)
        {
            _crossRatesSettingsService = crossRatesSettingsService;
            _assetPairSourceTypeService = assetPairSourceTypeService;
            _dependentCrossRatesService = dependentCrossRatesService;
        }

        /// <summary>
        ///     Replaces all existing settings.
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetCrossRatesSettings")]
        public IActionResult Set([FromBody] IEnumerable<CrossRatesSettingsModel> settings)
        {
            _crossRatesSettingsService.Set(settings.Select(Convert).ToList());
            return Ok(new {success = true});
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