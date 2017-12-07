using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class AssetPairsController : Controller, IAssetPairsApi
    {
        private readonly IAssetPairSourceTypeService _assetPairSourceTypeService;

        public AssetPairsController(IAssetPairSourceTypeService assetPairSourceTypeService)
        {
            _assetPairSourceTypeService = assetPairSourceTypeService;
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
        public void Add([NotNull] string assetPairId, AssetPairQuotesSourceTypeEnum sourceType)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.AddAssetPairQuotesSource(assetPairId, sourceType);
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
        public void Update([NotNull] [FromBody] AssetPairInputModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _assetPairSourceTypeService.UpdateAssetPairQuotesSource(model.AssetPairId, model.SourceType);
        }

        /// <summary>
        /// Deletes settings for an asset pair
        /// </summary>
        [HttpDelete]
        [Route("{assetPairId}")]
        public void Delete([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            _assetPairSourceTypeService.Delete(assetPairId);
        }
    }
}