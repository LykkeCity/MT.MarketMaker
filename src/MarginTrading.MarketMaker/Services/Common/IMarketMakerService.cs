using System.Threading.Tasks;
using MarginTrading.MarketMaker.Messages;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface IMarketMakerService
    {
        Task ProcessNewExternalOrderbookAsync(ExternalExchangeOrderbookMessage orderbook);
        Task ProcessNewSpotOrderBookDataAsync(SpotOrderbookMessage orderbook);
        Task ProcessNewAvgSpotRate(string assetPairId, decimal bid, decimal ask);
        Task ProcessNewManualQuotes(string assetPairId, decimal bid, decimal ask);
    }
}