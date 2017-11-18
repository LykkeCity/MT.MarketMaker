using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IGenerateOrderbookService
    {
        [CanBeNull]
        Orderbook OnNewOrderbook(ExternalOrderbook orderbook);
    }
}