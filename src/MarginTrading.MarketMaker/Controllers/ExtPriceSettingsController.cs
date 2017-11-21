using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Models.Settings;
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
            _extPricesSettingsService.Update(setting.AssetPairId, Convert(setting), "settings was manually changed");
            return Ok(new { success = true });
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyList<AssetPairExtPriceSettingsModel> List()
        {
            return _extPricesSettingsService.Get().Select(p => Convert(p.Key, p.Value)).ToList();
        }

        /// <summary>
        /// Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [CanBeNull]
        public AssetPairExtPriceSettingsModel Get(string assetPairId)
        {
            return Convert(assetPairId, _extPricesSettingsService.Get(assetPairId));
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

        private AssetPairExtPriceSettingsModel Convert(string assetPairId, AssetPairExtPriceSettings setting)
        {
            AUTOMAP
        }

        private AssetPairExtPriceSettings Convert(AssetPairExtPriceSettingsModel setting)
        {
            AUTOMAP
        }
    }
}