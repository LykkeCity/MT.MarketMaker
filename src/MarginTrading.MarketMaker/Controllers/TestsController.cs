using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
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
        public IActionResult Add([FromBody] ImmutableList<TestSettingModel> settings)
        {
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
        public IActionResult Delete(string assetPairId, string exchange)
        {
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
        public ImmutableList<TestSettingModel> Get(string assetPairId, string exchange)
        {
            return _testingHelperService.Get(assetPairId, exchange);
        }

        [HttpPost]
        [Route("manual-price/{assetPairId}")]
        public async Task<IActionResult> SetManualPrice(string assetPairId, decimal bid, decimal ask)
        {
            _assetPairSourceTypeService.UpdateAssetPairQuotesSource(assetPairId,
                AssetPairQuotesSourceTypeEnum.Manual);
            await _marketMakerService.ProcessNewManualQuotes(assetPairId, bid, ask);
            return Ok(new {success = true});
        }
    }
}