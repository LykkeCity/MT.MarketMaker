using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class AssetPairsController : Controller
    {
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;
        private readonly IConvertService _convertService;

        public AssetPairsController(IAssetPairSourceTypeService assetPairSourceTypeService, IConvertService convertService)
        {
            _assetPairSourceTypeService = assetPairSourceTypeService;
            _convertService = convertService;
        }

        /// <summary>
        /// Gets all existing settings
        /// </summary>
        [HttpGet]
        public IReadOnlyList<AssetPairModel> List()
        {
            return _assetPairSourceTypeService.Get()
                .OrderBy(d => d.Key)
                .Select(d => new AssetPairModel {SourceType = d.Value.ToString(), AssetPairId = d.Key})
                .ToList();
        }

        /// <summary>
        /// Adds default settings for an asset pair
        /// </summary>
        [HttpPost]
        [Route("{assetPairId}")]
        public IActionResult Add([NotNull] string assetPairId, AssetPairQuotesSourceTypeDomainEnum sourceType)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.AddAssetPairQuotesSource(assetPairId, sourceType);
            return Ok(new {success = true});
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
            var sourceType = _assetPairSourceTypeService.Get(assetPairId);
            return sourceType != null 
                ? new AssetPairModel {SourceType = sourceType.ToString(), AssetPairId = assetPairId }
                : null;
        }

        /// <summary>
        /// Updates settings for an asset pair
        /// </summary>
        [HttpPut]
        public IActionResult Update([NotNull] [FromBody] AssetPairInputModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _assetPairSourceTypeService.UpdateAssetPairQuotesSource(model.AssetPairId, Convert(model.SourceType));
            return Ok(new {success = true});
        }

        /// <summary>
        /// Deletes settings for an asset pair
        /// </summary>
        [HttpDelete]
        [Route("{assetPairId}")]
        public IActionResult Delete([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.Delete(assetPairId);
            return Ok(new {success = true});
        }

        private AssetPairQuotesSourceTypeDomainEnum Convert(AssetPairQuotesSourceTypeEnum sourceType)
        {
            return _convertService.Convert<AssetPairQuotesSourceTypeEnum, AssetPairQuotesSourceTypeDomainEnum>(
                sourceType);
        }
    }
}