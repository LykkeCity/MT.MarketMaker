using System;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class TraceModel
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public string Group { get; set; }
        public string Message { get; set; }
        public string AssetPairId { get; set; }
        public string TraceGroup { get; set; }
        public object Data { get; set; }
    }
}