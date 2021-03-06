﻿using System.Collections.Generic;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IOutliersOrderbooksService
    {
        IReadOnlyList<ExternalOrderbook> FindOutliers(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> validOrderbooks);
    }
}
