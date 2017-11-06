using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates
{
    public interface ICrossRatesSettingsService
    {
        void Set(CrossRatesSettings model);
        CrossRatesSettings Get();
    }
}
