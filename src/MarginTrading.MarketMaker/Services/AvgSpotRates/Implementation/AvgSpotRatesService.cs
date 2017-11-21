using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.CandlesHistory.Client;
using Lykke.Service.CandlesHistory.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.Common;

namespace MarginTrading.MarketMaker.Services.AvgSpotRates.Implementation
{
    internal class AvgSpotRatesService : TimerPeriod
    {
        private readonly ICandleshistoryservice _candlesHistoryService;
        private readonly IMarketMakerService _marketMakerService;
        private readonly ISystem _system;

        public AvgSpotRatesService(ILog log, ICandleshistoryservice candlesHistoryService, ISystem system,
            IMarketMakerService marketMakerService)
            : base(Startup.ServiceName + '_' + nameof(AvgSpotRatesService),
                (int) TimeSpan.FromHours(1).TotalMilliseconds, log)
        {
            _candlesHistoryService = candlesHistoryService;
            _system = system;
            _marketMakerService = marketMakerService;
        }

        public override async Task Execute()
        {
            var now = _system.UtcNow;
            const string assetPairId = "LKKUSD";
            var avg = await GetAvg(assetPairId, PriceType.Mid, now);
            await _marketMakerService.ProcessNewAvgSpotRate(assetPairId, avg, avg);
            Trace.Write(assetPairId + " trace", $"Avg quotes for {assetPairId} sent: {avg}");
        }

        private async Task<decimal> GetAvg(string assetPairId, PriceType priceType, DateTime now)
        {
            var candlesHistory = await _candlesHistoryService.GetCandlesHistoryAsync(assetPairId, priceType,
                TimeInterval.Min5, now.AddHours(-12), now);
            return (decimal) candlesHistory.History.SelectMany(h => new[] {h.Open, h.Close}).Average();
        }
    }
}