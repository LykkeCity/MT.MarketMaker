using System;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class LogModel
    {
        public DateTime Time { get; set; }
        public string Group { get; set; }
        public string Message { get; set; }
    }
}
