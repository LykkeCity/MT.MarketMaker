using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.MarketMaker.Controllers
{
    [Route("api/[controller]")]
    public class ExtPriceStatusController : Controller
    {
        private readonly IExtPricesStatusService _extPricesStatusService;

        public ExtPriceStatusController(IExtPricesStatusService extPricesStatusService)
        {
            _extPricesStatusService = extPricesStatusService;
        }

        /// <summary>
        /// Gets all status
        /// </summary>
        [HttpGet]
        public IReadOnlyDictionary<string, IReadOnlyList<ExtPriceStatusModel>> List()
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
        public string GetLogs()
        {
            return string.Join("\r\n", Trace.GetLast());
        }

        /// <summary>
        /// Gets logs for asset pair
        /// </summary>
        [HttpGet]
        [Route("logs/{contains}")]
        [CanBeNull]
        public string GetLogsFiltered(string contains)
        {
            return string.Join("\r\n", Trace.GetLast().Where(l => l.Contains(contains)));
        }
    }
}