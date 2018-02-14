using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class HedgingPreferenceService : IHedgingPreferenceService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        public HedgingPreferenceService(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
        }

        public ImmutableSortedDictionary<string, decimal> Get(string assetPairId)
        {
            // for now - get from settings
            return _extPricesSettingsService.GetHedgingPreferences(assetPairId);
        }
    }
}