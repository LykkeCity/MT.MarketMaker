﻿using System;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class TestSettingModel
    {
        public string AssetPairId { get; set; }
        public string Exchange { get; set; }
        [CanBeNull]
        public Range Disable { get; set; }
        [CanBeNull]
        public RangeWithAmount<decimal> BidModification { get; set; }
        [CanBeNull]
        public RangeWithAmount<decimal> AskModification { get; set; }

        public class Range
        {
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }

            public TimeSpan Duration
            {
                set => End = (Start ?? DateTime.UtcNow) + value;
            }
        }

        public class RangeWithAmount<T>: Range
        {
            public T Amount { get; set; }
        }
    }
}
