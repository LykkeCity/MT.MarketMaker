using System;
using System.Net;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Controllers
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly ISettingsRootService _settingsRootService;

        public IsAliveController(ISettingsRootService settingsRootService)
        {
            _settingsRootService = settingsRootService;
        }

        /// <summary>
        ///     Checks service is alive
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        [ProducesResponseType(typeof(IsAliveResponse), (int) HttpStatusCode.OK)]
        public IActionResult Get()
        {
            // NOTE: Feel free to extend IsAliveResponse, to display job-specific indicators
            var root = _settingsRootService.Get(); // check settings exist
            return Ok(new IsAliveResponse
            {
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO"),
#if DEBUG
                IsDebug = true,
#else
                IsDebug = false,
#endif
                AssetPairsCount = root.AssetPairs.Count,
            });
        }
    }
}