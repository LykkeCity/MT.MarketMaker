using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common
{
    public interface ISettingsValidationService
    {
        void Validate(SettingsRoot root);
    }
}