using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure;
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
        private readonly IConvertService _convertService;

        public ExtPriceSettingsController(IExtPricesSettingsService extPricesSettingsService,
            IConvertService convertService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _convertService = convertService;
        }

        /// <summary>
        ///     Updates settings for an asset pair
        /// </summary>
        [HttpPost]
        public IActionResult Update(AssetPairExtPriceSettingsModel setting)
        {
            _extPricesSettingsService.Update(setting.AssetPairId, Convert(setting), "settings was manually changed");
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyList<AssetPairExtPriceSettingsModel> List()
        {
            return _extPricesSettingsService.Get().Select(p => Convert(p.Key, p.Value)).ToList();
        }

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [CanBeNull]
        public AssetPairExtPriceSettingsModel Get(string assetPairId)
        {
            return Convert(assetPairId, _extPricesSettingsService.Get(assetPairId));
        }

        /// <summary>
        ///     Gets all hedging preferences
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
            var model = _convertService.Convert<AssetPairExtPriceSettings, AssetPairExtPriceSettingsModel>(setting,
                o => o.ConfigureMap(MemberList.Source));
            model.AssetPairId = assetPairId;
            return model;
        }

        private AssetPairExtPriceSettings Convert(AssetPairExtPriceSettingsModel model)
        {
            return _convertService.Convert<AssetPairExtPriceSettingsModel, AssetPairExtPriceSettings>(model,
                o => o.ConfigureMap(MemberList.Destination));
        }
    }
}