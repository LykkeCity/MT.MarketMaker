using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
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
        void ChangeExchangesTemporarilyDisabled(string assetPairId, ImmutableHashSet<string> exchanges, bool disable, string reason);
        bool IsExchangeConfigured(string assetPairId, string exchangeName);
        TimeSpan GetMinOrderbooksSendingPeriod(string assetPairId);

        void UpdateWithoutExchanges(string assetPairId, AssetPairExtPriceSettings setting, string reason);
        ImmutableDictionary<string, AssetPairExtPriceSettings> Get();
        [CanBeNull] AssetPairExtPriceSettings Get(string assetPairId);
        AssetPairExtPriceSettings GetDefault();
        ImmutableDictionary<OrderbookGeneratorStepEnum, bool> GetDefaultSteps();
        ImmutableDictionary<string, ImmutableDictionary<string, ExchangeExtPriceSettings>> GetExchanges();
        ExchangeExtPriceSettings Get(string assetPairId, string exchangeName);
        ExchangeExtPriceSettings Add(string assetPairId, string exchangeName, string reason);
        void Update(string assetPairId, string exchangeName, ExchangeExtPriceSettings settings, string reason);
        void Delete(string assetPairId, string exchangeName, string reason);
    }
}
