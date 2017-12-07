using System;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class RepeatedOutliersParamsModel
    {
        public int MaxSequenceLength { get; set; }
        public TimeSpan MaxSequenceAge { get; set; }
        public decimal MaxAvg { get; set; }
        public TimeSpan MaxAvgAge { get; set; }
    }
}