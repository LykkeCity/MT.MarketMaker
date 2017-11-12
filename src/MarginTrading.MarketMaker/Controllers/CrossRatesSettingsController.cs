using System.Collections.Generic;
using System.Linq;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.CrossRates;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class CrossRatesSettingsController : Controller
    {
        private readonly ICrossRatesSettingsService _crossRatesSettingsService;

        public CrossRatesSettingsController(ICrossRatesSettingsService crossRatesSettingsService)
        {
            _crossRatesSettingsService = crossRatesSettingsService;
        }

        /// <summary>
        ///     Inserts or updates settings for an asset pair
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