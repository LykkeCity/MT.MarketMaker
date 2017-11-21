using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class DisabledOrderbooksService : IDisabledOrderbooksService
    {
        private readonly IExtPricesSettingsService _extPricesSettingsService;

        public DisabledOrderbooksService(IExtPricesSettingsService extPricesSettingsService)
        {
            _extPricesSettingsService = extPricesSettingsService;
        }

        public ImmutableHashSet<string> GetDisabledExchanges(string assetPairId)
        {
            return _extPricesSettingsService.GetDisabledExchanges(assetPairId);
        }

        public void Disable(string assetPairId, ImmutableHashSet<string> exchanges, string reason)
        {
            _extPricesSettingsService.ChangeExchangesTemporarilyDisabled(assetPairId, exchanges, true, reason);
        }
    }
}
