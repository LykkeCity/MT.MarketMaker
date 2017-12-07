using Refit;

namespace MarginTrading.MarketMaker.Contracts.Client
{
    internal class MtMarketMakerClient : IMtMarketMakerClient
    {
        public IAssetPairsApi AssetPairs { get; }
        public IExtPriceExchangesApi ExtPriceExchanges { get; }
        public IExtPriceSettingsApi ExtPriceSettings { get; }
        public IExtPriceStatusApi ExtPriceStatus { get; }
        public ISettingsRootApi SettingsRoot { get; }

        public MtMarketMakerClient(string url, string userAgent)
        {
            var httpMessageHandler = new UserAgentHttpClientHandler(userAgent);
            var settings = new RefitSettings {HttpMessageHandlerFactory = () => httpMessageHandler};
            AssetPairs = RestService.For<IAssetPairsApi>(url, settings);
            ExtPriceExchanges = RestService.For<IExtPriceExchangesApi>(url, settings);
            ExtPriceSettings = RestService.For<IExtPriceSettingsApi>(url, settings);
            ExtPriceStatus = RestService.For<IExtPriceStatusApi>(url, settings);
            SettingsRoot = RestService.For<ISettingsRootApi>(url, settings);
        }
    }
}