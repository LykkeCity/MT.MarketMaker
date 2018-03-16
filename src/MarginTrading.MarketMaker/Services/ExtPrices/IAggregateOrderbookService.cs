using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IAggregateOrderbookService
    {
        Orderbook Aggregate(Orderbook originalOrderbook);
    }
}