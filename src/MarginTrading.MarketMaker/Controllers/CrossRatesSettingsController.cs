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

        public CrossRatesSettingsController(ICrossRatesSettingsService crossRatesSettingsService)
        {
            _crossRatesSettingsService = crossRatesSettingsService;
        }

        /// <summary>
        ///     Inserts or updates settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetCrossCourceSettings")]
        public IActionResult Set([FromBody] CrossRatesSettingsModel settings)
        {
            _crossRatesSettingsService.Set(Convert(settings));
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetCrossCourceSettings")]
        public CrossRatesSettingsModel Get()
        {
            return Convert(_crossRatesSettingsService.Get());
        }

        private static CrossRatesSettingsModel Convert(CrossRatesSettings settings)
        {
            return new CrossRatesSettingsModel
            {
                BaseAssetsIds = settings.BaseAssetsIds,
                OtherAssetsIds = settings.OtherAssetsIds,
            };
        }

        private static CrossRatesSettings Convert(CrossRatesSettingsModel model)
        {
            return new CrossRatesSettings(model.BaseAssetsIds, model.OtherAssetsIds);
        }
    }
}