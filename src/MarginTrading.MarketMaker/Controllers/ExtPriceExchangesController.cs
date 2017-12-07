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
    public class ExtPriceExchangesController : Controller
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IConvertService _convertService;

        public ExtPriceExchangesController(IExtPricesSettingsService extPricesSettingsService, IConvertService convertService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _convertService = convertService;
        }

        /// <summary>
        /// Gets all exchanges
        /// </summary>
        [HttpGet]
        public IReadOnlyList<ExchangeExtPriceSettingsModel> List()
        {
            return _extPricesSettingsService.GetExchanges()
                .SelectMany(a => a.Value.Select(e => Convert(a.Key, e.Key, e.Value)))
                .OrderBy(a => a.AssetPairId).ThenBy(a => a.ExchangeName)
                .ToList();
        }
        
        /// <summary>
        /// Updates an exchange
        /// </summary>
        [HttpPut]
        public IActionResult Update([NotNull] [FromBody] ExchangeExtPriceSettingsModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _extPricesSettingsService.Update(model.AssetPairId, model.ExchangeName, Convert(model),
                "settings was manually changed");
            return Ok(new {success = true});
        }

        /// <summary>
        /// Gets all exchanges for an asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        public IReadOnlyList<ExchangeExtPriceSettingsModel> ListByAssetPair([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _extPricesSettingsService.Get(assetPairId)?.Exchanges
                       .Select(e => Convert(assetPairId, e.Key, e.Value))
                       .OrderBy(a => a.ExchangeName).ToList()
                   ?? new List<ExchangeExtPriceSettingsModel>();
        }

        /// <summary>
        /// Gets exchange by asset pair and exchange name
        /// </summary>
        [CanBeNull]
        [HttpGet]
        [Route("{assetPairId}/{exchangeName}")]
        public ExchangeExtPriceSettingsModel Get([NotNull] string assetPairId, [NotNull] string exchangeName)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            return Convert(assetPairId, exchangeName, _extPricesSettingsService.Get(assetPairId, exchangeName));
        }

        /// <summary>
        /// Adds an exchange with default settings
        /// </summary>
        [HttpPost]        
        [Route("{assetPairId}/{exchangeName}")]
        public ExchangeExtPriceSettingsModel Add([NotNull] string assetPairId, [NotNull] string exchangeName)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            var settings = _extPricesSettingsService.Add(assetPairId, exchangeName, "settings was manually changed");
            return Convert(assetPairId, exchangeName, settings);
        }

        /// <summary>
        /// Deletes an exchange
        /// </summary>
        [HttpDelete]        
        [Route("{assetPairId}/{exchangeName}")]
        public IActionResult Delete([NotNull] string assetPairId, [NotNull] string exchangeName)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            _extPricesSettingsService.Delete(assetPairId, exchangeName, "settings was manually changed");
            return Ok(new {success = true});
        }

        [CanBeNull]
        private ExchangeExtPriceSettingsModel Convert(string assetPairId, string exchangeName, [CanBeNull] ExchangeExtPriceSettings settings)
        {
            if (settings == null)
                return null;
            
            var model = _convertService.Convert<ExchangeExtPriceSettings, ExchangeExtPriceSettingsModel>(settings, o => o.ConfigureMap(MemberList.Source));
            model.ExchangeName = exchangeName;
            model.AssetPairId = assetPairId;
            return model;
        }

        private ExchangeExtPriceSettings Convert(ExchangeExtPriceSettingsModel settings)
        {
            return _convertService.Convert<ExchangeExtPriceSettingsModel, ExchangeExtPriceSettings>(settings, o => o.ConfigureMap(MemberList.Destination));
        }
    }
}