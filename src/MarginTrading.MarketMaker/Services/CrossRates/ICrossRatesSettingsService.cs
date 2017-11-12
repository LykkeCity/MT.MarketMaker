using System.Collections.Generic;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface ICrossRatesSettingsService
    {
        void Set(IReadOnlyList<CrossRatesSettings> model);
        IReadOnlyList<CrossRatesSettings> Get();
    }
}
