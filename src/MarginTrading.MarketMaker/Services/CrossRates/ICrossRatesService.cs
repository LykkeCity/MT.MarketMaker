using System.Collections.Immutable;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    internal interface ICrossRatesService
    {
        ImmutableList<Orderbook> CalcDependentOrderbooks(Orderbook orderbook);
    }
}