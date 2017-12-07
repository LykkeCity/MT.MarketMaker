using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Contracts
{
    [PublicAPI]
    public interface IMtMarketMakerClient
    {
        IAssetPairsApi AssetPairs { get; }
        IExtPriceExchangesApi ExtPriceExchanges { get; }
        IExtPriceSettingsApi ExtPriceSettings { get; }
        IExtPriceStatusApi ExtPriceStatus { get; }
        ISettingsRootApi SettingsRoot { get; }
    }
}