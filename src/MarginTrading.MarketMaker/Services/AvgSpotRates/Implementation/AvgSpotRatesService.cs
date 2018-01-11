using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.CandlesHistory.Client;
using Lykke.Service.CandlesHistory.Client.Models;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.Common;

namespace MarginTrading.MarketMaker.Services.AvgSpotRates.Implementation
{
    internal class AvgSpotRatesService : TimerPeriod, IAvgSpotRatesService
    {
        private readonly ICandleshistoryservice _candlesHistoryService;
        private readonly IMarketMakerService _marketMakerService;
        private readonly IPriceRoundingService _priceRoundingService;
        private readonly ISystem _system;
        private readonly ILog _log;
        
        public AvgSpotRatesService(ILog log, ICandleshistoryservice candlesHistoryService, ISystem system,
            IMarketMakerService marketMakerService, IPriceRoundingService priceRoundingService)
            : base(Startup.ServiceName + '_' + nameof(AvgSpotRatesService),
                (int) TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _candlesHistoryService = candlesHistoryService;
            _system = system;
            _marketMakerService = marketMakerService;
            _priceRoundingService = priceRoundingService;
            _log = log;
        }

        public override async Task Execute()
        {
            var now = _system.UtcNow;
            const string assetPairId = "LKKUSD";
            var avg = await GetAvg(assetPairId, CandlePriceType.Mid, now);
            if (avg == null)
            {
                return;
            }
            
            await _marketMakerService.ProcessNewAvgSpotRate(assetPairId, avg.Value, avg.Value);
            Trace.Write(TraceLevelGroupEnum.Trace, assetPairId, $"Avg spot quotes sent: {avg}",
                new {avg.Value, Event = "AvgSpotRatesSent"});
        }

        [ItemCanBeNull] 
        private async Task<decimal?> GetAvg(string assetPairId, CandlePriceType priceType, DateTime now)
        {
            var candlesHistory = await _candlesHistoryService.GetCandlesHistoryAsync(assetPairId, priceType,
                CandleTimeInterval.Min5, now.AddHours(-12), now);
            if (!candlesHistory.History.Any())
            {
                _log.WriteErrorAsync(GetComponentName(), nameof(GetAvg),
                    new Exception("No candles history found for " + assetPairId)
                        {Data = {{"AssetPairId", assetPairId}}});
                return null;
            }

            var average = (decimal) candlesHistory.History.SelectMany(h => new[] {h.Open, h.Close}).Average();
            return _priceRoundingService.Round(assetPairId, average);
        }

        public void Run()
        {
            Start();
        }
    }
}