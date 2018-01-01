using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class ExtPriceSettingsController : Controller
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IExtPricesStatusService _extPricesStatusService;
        private readonly IConvertService _convertService;

        public ExtPriceSettingsController(IExtPricesSettingsService extPricesSettingsService,
            IConvertService convertService, IExtPricesStatusService extPricesStatusService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _convertService = convertService;
            _extPricesStatusService = extPricesStatusService;
        }

        /// <summary>
        ///     Updates settings for an asset pair
        /// </summary>
        [HttpPut]
        public IActionResult Update([NotNull] [FromBody] AssetPairExtPriceSettingsModel setting)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));
            _extPricesSettingsService.UpdateWithoutExchanges(setting.AssetPairId, Convert(setting),
                "settings was manually changed");
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyList<AssetPairExtPriceSettingsModel> List()
        {
            return _extPricesSettingsService.Get().Select(p => Convert(p.Key, p.Value)).OrderBy(m => m.AssetPairId)
                .ToList();
        }

        /// <summary>
        ///     Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [CanBeNull]
        public AssetPairExtPriceSettingsModel Get([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return Convert(assetPairId, _extPricesSettingsService.Get(assetPairId));
        }

        /// <summary>
        ///     Gets all hedging preferences
        /// </summary>
        [HttpGet]
        [Route("hedging-preferences")]
        public IReadOnlyList<HedgingPreferenceModel> GetHedgingPreferences()
        {
            var statusDict = _extPricesStatusService.Get().ToDictionary(s => (s.AssetPairId, s.ExchangeName));
            return _extPricesSettingsService.Get().SelectMany(asset =>
            {
                return asset.Value.Exchanges.Select(exchange =>
                {
                    var status = statusDict.GetValueOrDefault((asset.Key, exchange.Key));
                    return new HedgingPreferenceModel
                    {
                        AssetPairId = asset.Key,
                        Exchange = exchange.Key,
                        IsHedgingUnavailable = exchange.Value.Hedging.IsTemporarilyUnavailable || exchange.Value.Disabled.IsTemporarilyDisabled,
                        IsPrimary = status?.IsPrimary == true,
                        ErrorState = status?.ErrorState ?? "NoOrderbook",
                        Preference = exchange.Value.Hedging.DefaultPreference,
                    };
                });
            }).ToList();
        }

        [CanBeNull]
        private AssetPairExtPriceSettingsModel Convert(string assetPairId,
            [CanBeNull] AssetPairExtPriceSettings setting)
        {
            if (setting == null)
                return null;

            var model = _convertService.Convert<AssetPairExtPriceSettings, AssetPairExtPriceSettingsModel>(setting,
                o => o.ConfigureMap(MemberList.Source).ForSourceMember(s => s.Exchanges, e => e.Ignore()));
            model.AssetPairId = assetPairId;
            model.Steps = _extPricesSettingsService.GetDefaultSteps().SetItems(setting.Steps);
            return model;
        }

        private AssetPairExtPriceSettings Convert(AssetPairExtPriceSettingsModel model)
        {
            var settings = _convertService.Convert<AssetPairExtPriceSettingsModel, AssetPairExtPriceSettings>(model,
                o => o.ConfigureMap(MemberList.Destination).ForCtorParam("exchanges",
                        e => e.ResolveUsing(m => ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty))
                    .ForMember(e => e.Exchanges, e => e.Ignore()));
            return settings;
        }
    }
}