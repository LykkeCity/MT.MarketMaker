﻿namespace MarginTrading.MarketMaker.Services.CrossRates.Models
{
    internal class CrossRateSourceAssetPair
    {
        public string Id { get; }
        public bool IsUsdQuoting { get; }

        public CrossRateSourceAssetPair(string id, bool isUsdQuoting)
        {
            Id = id;
            IsUsdQuoting = isUsdQuoting;
        }
    }
}