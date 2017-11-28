using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class CrossRateCalcInfosController : Controller
    {
        private readonly ICrossRateCalcInfosService _crossRateCalcInfosService;
        private readonly IConvertService _convertService;

        public CrossRateCalcInfosController(ICrossRateCalcInfosService crossRateCalcInfosService,
            IConvertService convertService)
        {
            _crossRateCalcInfosService = crossRateCalcInfosService;
            _convertService = convertService;
        }

        /// <summary>
        ///     Update setting for a resulting cross-pair
        /// </summary>
        [HttpPost]
        public IActionResult Update([NotNull] [FromBody] CrossRateCalcInfoModel settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _crossRateCalcInfosService.Update(Convert(settings));
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public IEnumerable<CrossRateCalcInfoModel> List()
        {
            return _crossRateCalcInfosService.Get().Values.Select(Convert);
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        public CrossRateCalcInfoModel Get([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return Convert(_crossRateCalcInfosService.Get(assetPairId));
        }

        private CrossRateCalcInfoModel Convert(CrossRateCalcInfo settings)
        {
            return _convertService.Convert<CrossRateCalcInfo, CrossRateCalcInfoModel>(settings);
        }

        private CrossRateCalcInfo Convert(CrossRateCalcInfoModel model)
        {
            return _convertService.Convert<CrossRateCalcInfoModel, CrossRateCalcInfo>(model);
        }
    }
}