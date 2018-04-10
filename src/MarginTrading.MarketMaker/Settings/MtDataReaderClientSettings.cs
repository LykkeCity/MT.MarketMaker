using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    public class MtDataReaderClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
        public string ApiKey { get; set; }
    }
}