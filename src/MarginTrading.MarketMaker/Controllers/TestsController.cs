using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Filters;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    [TestFunctionalityFilter]
    public class TestsController : Controller
    {
        private readonly ITestingHelperService _testingHelperService;
        private readonly IMarketMakerService _marketMakerService;
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;

        public TestsController(ITestingHelperService testingHelperService, IMarketMakerService marketMakerService,
            IAssetPairSourceTypeService assetPairSourceTypeService)
        {
            _testingHelperService = testingHelperService;
            _marketMakerService = marketMakerService;
            _assetPairSourceTypeService = assetPairSourceTypeService;
        }

        /// <summary>
        ///     Adds settings
        /// </summary>
        [HttpPut]
        public IActionResult Add([NotNull] [FromBody] ImmutableList<TestSettingModel> settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _testingHelperService.Add(settings);
            return Ok(new {success = true});
        }


        /// <summary>
        ///     Deletes settings
        /// </summary>
        [HttpDelete]
        public IActionResult DeleteAll()
        {
            _testingHelperService.DeleteAll();
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Deletes settings
        /// </summary>
        [HttpDelete]
        [Route("{assetPairId}/{exchange}")]
        public IActionResult Delete([NotNull] string assetPairId, [NotNull] string exchange)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            _testingHelperService.Delete(assetPairId, exchange);
            return Ok(new {success = true});
        }

        /// <summary>
        ///     Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyDictionary<(string AssetPairId, string Exchange), ImmutableList<TestSettingModel>> List()
        {
            return _testingHelperService.GetAll();
        }

        /// <summary>
        ///     Get settings
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}/{exchange}")]
        public ImmutableList<TestSettingModel> Get([NotNull] string assetPairId, [NotNull] string exchange)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            return _testingHelperService.Get(assetPairId, exchange);
        }

        /// <summary>
        /// Sets quotes source type to manual and sends manual quotes 
        /// </summary>
        [HttpPost]
        [Route("manual-price/{assetPairId}")]
        public async Task<IActionResult> SetManualPrice([NotNull] string assetPairId, decimal bid, decimal ask)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.UpdateAssetPairQuotesSource(assetPairId,
                AssetPairQuotesSourceTypeEnum.Manual);
            await _marketMakerService.ProcessNewManualQuotes(assetPairId, bid, ask);
            return Ok(new {success = true});
        }
    }
}