using System;
using System.Collections.Immutable;
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
    public class ExtPriceExchangesController : Controller
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IConvertService _convertService;

        public ExtPriceExchangesController(IExtPricesSettingsService extPricesSettingsService, IConvertService convertService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _convertService = convertService;
        }

        [HttpGet]
        public ImmutableDictionary<string, ImmutableDictionary<string, ExchangeExtPriceSettingsModel>> List()
        {
            return _extPricesSettingsService.GetExchanges()
                    .ToImmutableDictionary(a => a.Key, a => a.Value.ToImmutableDictionary(e => e.Key, e => Convert(a.Key, e.Key, e.Value)));
        }

        [HttpGet]
        [Route("{assetPairId}")]
        public ImmutableDictionary<string, ExchangeExtPriceSettingsModel> List([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _extPricesSettingsService.Get(assetPairId)?.Exchanges.ToImmutableDictionary(e => e.Key, e => Convert(assetPairId, e.Key, e.Value))
                ?? ImmutableDictionary<string, ExchangeExtPriceSettingsModel>.Empty;
        }

        [CanBeNull]
        [HttpGet]        
        [Route("{assetPairId}/{exchangeName}")]
        public ExchangeExtPriceSettingsModel Get([NotNull] string assetPairId, [NotNull] string exchangeName)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            return Convert(assetPairId, exchangeName, _extPricesSettingsService.Get(assetPairId, exchangeName));
        }

        [HttpPut]        
        [Route("{assetPairId}/{exchangeName}")]
        public ExchangeExtPriceSettingsModel Add([NotNull] string assetPairId, [NotNull] string exchangeName)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchangeName == null) throw new ArgumentNullException(nameof(exchangeName));
            return Convert(assetPairId, exchangeName, _extPricesSettingsService.Add(assetPairId, exchangeName, "settings was manually changed"));
        }
        
        [HttpPost]
        public IActionResult Add([NotNull] [FromBody] ExchangeExtPriceSettingsModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _extPricesSettingsService.Update(model.AssetPairId, model.ExchangeName, Convert(model),
                "settings was manually changed");
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