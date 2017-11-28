using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class AssetPairController : Controller
    {
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;

        public AssetPairController(IAssetPairSourceTypeService assetPairSourceTypeService)
        {
            _assetPairSourceTypeService = assetPairSourceTypeService;
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        public ImmutableDictionary<string, AssetPairModel> List()
        {
            return _assetPairSourceTypeService.Get()
                .ToImmutableDictionary(d => d.Key, d => new AssetPairModel {SourceType = d.Value.ToString()});
        }

        /// <summary>
        /// Gets settings for a single asset pair
        /// </summary>
        [CanBeNull]
        [HttpGet]
        [Route("{assetPairId}")]
        public AssetPairModel Get([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return new AssetPairModel {SourceType = _assetPairSourceTypeService.Get(assetPairId).ToString()};
        }

        /// <summary>
        /// Inserts settings for an asset pair
        /// </summary>
        [HttpPut]
        [Route("{assetPairId}")]
        public IActionResult Add([NotNull] string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.AddAssetPairQuotesSource(assetPairId, sourceType);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Updates settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("{assetPairId}")]
        public IActionResult Update([NotNull] string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.UpdateAssetPairQuotesSource(assetPairId, sourceType);
            return Ok(new { success = true });
        }
    }
}