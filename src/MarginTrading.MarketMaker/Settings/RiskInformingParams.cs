using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Settings
{
    public class RiskInformingParams
    {
        public string System { get; set; }
        public string EventTypeCode { get; set; }
        public AlertSeverityLevel Level { get; set; }
        public string Message { get; set; }
    }
}