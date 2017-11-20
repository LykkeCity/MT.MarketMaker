using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private readonly IMarketMakerService _marketMakerService;
        private readonly IAssetPairsSettingsService _assetPairsSettingsService;

        public SettingsController(IMarketMakerService marketMakerService, IAssetPairsSettingsService assetPairsSettingsService)
        {
            _marketMakerService = marketMakerService;
            _assetPairsSettingsService = assetPairsSettingsService;
        }

        /// <summary>
        /// Inserts or updates settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("set")]
        [SwaggerOperation("SetSettings")]
        public async Task<IActionResult> Set([FromBody] AssetPairSettingsInputModel settings)
        {
            await _marketMakerService.ProcessAssetPairSettingsAsync(settings);
            return Ok(new {success = true});
        }

        /// <summary>
        /// Inserts or updates settings array
        /// </summary>
        [HttpPost]
        [Route("set-array")]
        [SwaggerOperation("SetSettingsArray")]
        public async Task<IActionResult> Set([FromBody] IReadOnlyList<AssetPairSettingsInputModel> settings)
        {
            foreach (var setting in settings)
            {
                await _marketMakerService.ProcessAssetPairSettingsAsync(setting);
            }

            return Ok(new {success = true});
        }


        /// <summary>
        /// Deletes settings for an asset pair. This makes the pair get ignored.
        /// </summary>
        [HttpPost]
        [Route("delete/{assetPairId}")]
        [SwaggerOperation("DeleteSettings")]
        public async Task<IActionResult> Delete(string assetPairId)
        {
            await _assetPairsSettingsService.DeleteAsync(assetPairId);
            return Ok(new {success = true});
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        [Route("")]
        [SwaggerOperation("GetAllSettings")]
        public async Task<List<AssetPairSettingsOutputModel>> GetAll()
        {
            return (await _assetPairsSettingsService.GetAllPairsSourcesAsync())
                .Select(s => new AssetPairSettingsOutputModel
                {
                    AssetPairId = s.AssetPairId,
                    QuotesSourceType = s.QuotesSourceType.ToString(),
                }).ToList();

        }

        /// <summary>
        /// Gets settings for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [SwaggerOperation("GetSettings")]
        [CanBeNull]
        public AssetPairSettingsOutputModel Get(string assetPairId)
        {
            var source = _assetPairsSettingsService.Get(assetPairId);
            return new AssetPairSettingsOutputModel
            {
                AssetPairId = source.AssetPairId,
                QuotesSourceType = source.QuotesSourceType.ToString(),
            };
        }
    }
}