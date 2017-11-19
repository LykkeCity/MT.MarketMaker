using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;

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
        /// Inserts settings for an asset pair
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Add(AssetPairExtPriceSettingsModel setting)
        {
            await _extPricesSettingsService.AddAsync(setting);
            return Ok(new {success = true});
        }

        /// <summary>
        /// Updates settings for an asset pair
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Update(AssetPairExtPriceSettingsModel setting)
        {
            await _extPricesSettingsService.UpdateAsync(setting);
            return Ok(new {success = true});
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyList<AssetPairExtPriceSettingsModel> List()
        {
            return _extPricesSettingsService.Get(); AUTOMAP
        }

        /// <summary>
        /// Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [CanBeNull]
        public AssetPairExtPriceSettingsModel Get(string assetPairId)
        {
            return _extPricesSettingsService.Get(assetPairId); AUTOMAP
        }

        /// <summary>
        /// Gets all hedging preferences
        /// </summary>
        [HttpGet]
        [Route("hedging-preferences")]
        public IReadOnlyList<HedgingPreferenceModel> GetHedgingPreferences()
        {
            _extPricesSettingsService.Get().SelectMany(ap =>
                ap.Values.Select(e => AUTOMAP)).ToList();
        }
    }
}