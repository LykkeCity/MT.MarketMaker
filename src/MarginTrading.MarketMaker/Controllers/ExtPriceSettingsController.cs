using System.Collections.Generic;
using System.Linq;
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
        /// Updates settings for an asset pair
        /// </summary>
        [HttpPost]
        public IActionResult Update(AssetPairExtPriceSettingsModel setting)
        {
            _extPricesSettingsService.Update(setting); AUTOMAP
            return Ok(new { success = true });
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
            return _extPricesSettingsService.Get().SelectMany(ap =>
                ap.Value.Exchanges.Select(e => new HedgingPreferenceModel
                {
                    AssetPairId = ap.Key,
                    Exchange = e.Key,
                    HedgingTemporarilyDisabled = e.Value.Hedging.IsTemporarilyUnavailable,
                    Preference = e.Value.Hedging.DefaultPreference,
                })).ToList();
        }
    }
}