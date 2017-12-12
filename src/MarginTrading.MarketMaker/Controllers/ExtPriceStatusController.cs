using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class ExtPriceStatusController : Controller
    {
        private readonly IExtPricesStatusService _extPricesStatusService;
        private readonly ITraceService _traceService;

        public ExtPriceStatusController(IExtPricesStatusService extPricesStatusService, ITraceService traceService)
        {
            _extPricesStatusService = extPricesStatusService;
            _traceService = traceService;
        }

        /// <summary>
        /// Gets all status
        /// </summary>
        [HttpGet]
        public IReadOnlyList<ExtPriceStatusModel> List()
        {
            return _extPricesStatusService.Get();
        }

        /// <summary>
        /// Gets status for a single asset pair
        /// </summary>
        [HttpGet]
        [Route("{assetPairId}")]
        [CanBeNull]
        public IReadOnlyList<ExtPriceStatusModel> Get(string assetPairId)
        {
            return _extPricesStatusService.Get(assetPairId);
        }

        /// <summary>
        /// Gets logs
        /// </summary>
        [HttpGet]
        [Route("logs")]
        [CanBeNull]
        public IReadOnlyList<TraceModel> GetLogs()
        {
            return _traceService.GetLast();
        }

        /// <summary>
        /// Gets logs for asset pair
        /// </summary>
        [HttpGet]
        [Route("logs/{contains}")]
        [CanBeNull]
        public IReadOnlyList<TraceModel> GetLogsFiltered(string contains)
        {
            return _traceService.GetLast(contains);
        }
    }
}