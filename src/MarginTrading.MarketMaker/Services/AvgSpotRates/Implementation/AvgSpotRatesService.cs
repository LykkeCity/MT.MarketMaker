using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.CandlesHistory.Client;
using Lykke.Service.CandlesHistory.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;

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
            var avgBid = await GetAvg(assetPairId, PriceType.Bid, now);
            var avgAsk = await GetAvg(assetPairId, PriceType.Ask, now);
            await _marketMakerService.ProcessNewAvgSpotRate(assetPairId, avgBid, avgAsk);
            Trace.Write(assetPairId + " trace", $"Avg quotes for {assetPairId} sent", new {avgBid, avgAsk});
        }

        private async Task<decimal> GetAvg(string assetPairId, PriceType priceType, DateTime now)
        {
            var candlesHistory = await _candlesHistoryService.GetCandlesHistoryAsync(assetPairId, priceType,
                TimeInterval.Min5, now.AddHours(-12), now);
            return (decimal) candlesHistory.History.SelectMany(h => new[] {h.Open, h.Close}).Average();
        }
    }
}