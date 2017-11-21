﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IExtPricesSettingsService
    {
        bool IsStepEnabled(OrderbookGeneratorStepEnum step, string assetPairId);
        string GetPresetPrimaryExchange(string assetPairId);
        decimal GetVolumeMultiplier(string assetPairId, string exchangeName);
        TimeSpan GetOrderbookOutdatingThreshold(string assetPairId, string exchangeName, DateTime now);
        RepeatedOutliersParams GetRepeatedOutliersParams(string assetPairId);
        decimal GetOutlierThreshold(string assetPairId);
        ImmutableDictionary<string, decimal> GetHedgingPreferences(string assetPairId);
        (decimal Bid, decimal Ask) GetPriceMarkups(string assetPairId);
        ImmutableHashSet<string> GetDisabledExchanges(string assetPairId);
        Task ChangeExchangesTemporarilyDisabled(string assetPairId, ImmutableHashSet<string> exchanges, bool disable, string reason);
        bool IsExchangeConfigured(string assetPairId, string exchange);
        TimeSpan GetMinOrderbooksSendingPeriod(string assetPairId);

        Task AddAsync(AssetPairExtPriceSettings setting);
        Task UpdateAsync(AssetPairExtPriceSettings setting);
        ImmutableDictionary<string, AssetPairExtPriceSettings> Get();
        AssetPairExtPriceSettings Get(string assetPairId);
        AssetPairExtPriceSettings GetDefault();
    }
}