using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class ExtPriceSettingsController : Controller
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        public ExtPriceSettingsController(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
        }

        /// <summary>
        /// Inserts or updates settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetExtPriceSettings")]
        public async Task<IActionResult> Set([FromBody] IEnumerable<AssetPairExtPriceSettingsModel> settings)
        {
            await Task.WhenAll(settings.Select(s => _extPricesSettingsService.Set(s)));
            return Ok(new {success = true});
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetAllExtPriceSettings")]
        public Task<IReadOnlyList<AssetPairExtPriceSettingsModel>> GetAll()
        {
            return _extPricesSettingsService.GetAllAsync();
        }

        /// <summary>
        /// Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [SwaggerOperation("GetExtPriceSettings")]
        [CanBeNull]
        public Task<IReadOnlyList<AssetPairExtPriceSettingsModel>> Get(string assetPairId)
        {
            return _extPricesSettingsService.GetAllAsync(assetPairId);
        }

        /// <summary>
        /// Gets all hedging preferences
        /// </summary>
        [HttpGet]
        [Route("hedging-preferences")]
        [SwaggerOperation("GetAllExtHedgingPreferences")]
        public IReadOnlyList<HedgingPreferenceModel> GetAllHedgingPreferences()
        {
            return _extPricesSettingsService.GetAllHedgingPreferences();
        }
    }
}