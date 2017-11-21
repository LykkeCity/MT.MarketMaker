using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Models.Settings;
using MoreLinq;
using MarginTrading.MarketMaker.Services.Common;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    internal class ExtPricesSettingsService : IExtPricesSettingsService
    {
        private readonly IAlertService _alertService;
        private readonly ISettingsRootService _settingsRootService;

        public ExtPricesSettingsService(IAlertService alertService, ISettingsRootService settingsRootService)
        {
            _alertService = alertService;
            _settingsRootService = settingsRootService;
        }

        public bool IsStepEnabled(OrderbookGeneratorStepEnum step, string assetPairId)
        {
            return GetAsset(assetPairId).Steps.GetValueOrDefault(step, true);
        }

        public string GetPresetPrimaryExchange(string assetPairId)
        {
            return GetAsset(assetPairId).PresetDefaultExchange;
        }

        public decimal GetVolumeMultiplier(string assetPairId, string exchangeName)
        {
            return GetExchange(assetPairId, exchangeName).OrderGeneration.VolumeMultiplier;
        }

        public TimeSpan GetOrderbookOutdatingThreshold(string assetPairId, string exchangeName, DateTime now)
        {
            return GetExchange(assetPairId, exchangeName).OrderbookOutdatingThreshold;
        }

        public RepeatedOutliersParams GetRepeatedOutliersParams(string assetPairId)
        {
            var p = GetAsset(assetPairId).RepeatedOutliers;
            return new RepeatedOutliersParams(p.MaxSequenceLength, p.MaxSequenceAge, p.MaxAvg, p.MaxAvgAge);
        }

        public decimal GetOutlierThreshold(string assetPairId)
        {
            return (decimal) GetAsset(assetPairId).OutlierThreshold;
        }

        public ImmutableDictionary<string, decimal> GetHedgingPreferences(string assetPairId)
        {
            return GetAllExchanges(assetPairId).ToImmutableDictionary(e => e.Key,
                e => e.Value.Hedging.IsTemporarilyUnavailable ? 0m : e.Value.Hedging.DefaultPreference);
        }

        public (decimal Bid, decimal Ask) GetPriceMarkups(string assetPairId)
        {
            var markups = GetAsset(assetPairId).Markups;
            return ((decimal) markups.Bid, (decimal) markups.Ask);
        }

        public ImmutableHashSet<string> GetDisabledExchanges(string assetPairId)
        {
            return GetAllExchanges(assetPairId).Where(p => p.Value.Disabled.IsTemporarilyDisabled).Select(p => p.Key)
                .ToImmutableHashSet();
        }

        public void ChangeExchangesTemporarilyDisabled(string assetPairId, ImmutableHashSet<string> exchanges, bool disable, string reason)
        {
            if (exchanges.Count == 0)
                return;

            UpdateExchanges(assetPairId, exchanges,
                old => new ExchangeExtPriceSettings(disabled: new ExchangeExtPriceSettings.DisabledSettings(disable, reason), ), reason);
        }

        private void ExchangesStateChanged(string assetPairId, IEnumerable<string> exchanges, string stateChangeDescription, string reason)
        {
            var exchangesStr = string.Join(", ", exchanges);
            if (exchangesStr == string.Empty)
            {
                return;
            }

            var ending = exchangesStr.Contains(',') ? "s" : "";

            _alertService.AlertRiskOfficer(assetPairId, $"Exchange{ending} {exchangesStr} for {assetPairId} became {stateChangeDescription} because {reason}");
        }

        public bool IsExchangeConfigured(string assetPairId, string exchangeName)
        {
            return TryGetExchange(assetPairId, exchangeName) != null;
        }

        public TimeSpan GetMinOrderbooksSendingPeriod(string assetPairId)
        {
            return GetAsset(assetPairId).MinOrderbooksSendingPeriod;
        }

        public void Update(AssetPairExtPriceSettings setting)
        {
            throw new NotImplementedException();
        }

        public ImmutableDictionary<string, AssetPairExtPriceSettings> Get()
        {
            throw new NotImplementedException();
        }

        public AssetPairExtPriceSettings Get(string assetPairId)
        {
            throw new NotImplementedException();
        }

        public AssetPairExtPriceSettings GetDefault()
        {
            return new AssetPairExtPriceSettings("",
                0.05m, TimeSpan.FromSeconds(1 / 2), new AssetPairExtPriceSettings.MarkupsParams(0, 0),
                new AssetPairExtPriceSettings.RepeatedOutliersParams(10, TimeSpan.FromMinutes(5), 10,
                    TimeSpan.FromMinutes(5)),
                GetDefaultSteps(), ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty);
        }

        private static bool CanPerformHedging(KeyValuePair<string, ExchangeExtPriceSettings> keyValuePair)
        {
            return keyValuePair.Value.Hedging.DefaultPreference * (keyValuePair.Value.Hedging.IsTemporarilyUnavailable ? 0 : 1) > 0;
        }

        private ImmutableDictionary<string, ExchangeExtPriceSettings> GetAllExchanges(string assetPairId)
        {
            return _settingsRootService.Get(assetPairId)?.ExtPriceSettings?.Exchanges
                   ?? ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty;
        }

        private ExchangeExtPriceSettings GetExchange(string assetPairId, string exchangeName)
        {
            return TryGetExchange(assetPairId, exchangeName)
                   ?? throw new ArgumentException(
                       $"Settings for exchange {exchangeName} for asset pair {assetPairId} not found", nameof(exchangeName));
        }

        [CanBeNull]
        private ExchangeExtPriceSettings TryGetExchange(string assetPairId, string exchangeName)
        {
            return GetAllExchanges(assetPairId).GetValueOrDefault(exchangeName);
        }

        private AssetPairExtPriceSettings GetAsset(string assetPairId)
        {
            return TryGetAsset(assetPairId)
                   ?? throw new ArgumentException($"Settings for asset pair {assetPairId} not found",
                       nameof(assetPairId));
        }

        [CanBeNull]
        private AssetPairExtPriceSettings TryGetAsset(string assetPairId)
        {
            return _settingsRootService.Get(assetPairId)?.ExtPriceSettings;
        }

        private void Update(string assetPairId,
            Func<AssetPairExtPriceSettings, AssetPairExtPriceSettings> changeFunc, string reason)
        {
            ImmutableDictionary<string, ExchangeExtPriceSettings> oldExchanges = null;
            ImmutableDictionary<string, ExchangeExtPriceSettings> newExchanges = null;
            _settingsRootService.Update(assetPairId, old =>
            {
                oldExchanges = old.ExtPriceSettings.Exchanges;
                var newSettings = changeFunc(old.ExtPriceSettings);
                newExchanges = newSettings.Exchanges;
                return new AssetPairSettings(old.QuotesSourceType, newSettings, old.CrossRateCalcInfo);
            });

            oldExchanges.RequiredNotNull(nameof(oldExchanges));
            newExchanges.RequiredNotNull(nameof(newExchanges));
            ExchangesTemporarilyDisabledChanged(assetPairId, oldExchanges, newExchanges, reason);
            CanPerformHedgingChanged(assetPairId, oldExchanges, newExchanges, reason);
        }

        private void UpdateExchanges(string assetPairId, IEnumerable<string> exchangeNames,
            Func<ExchangeExtPriceSettings, ExchangeExtPriceSettings> changeFunc, string reason)
        {

            var exchangesHashSet = exchangeNames.ToImmutableHashSet();
            Update(assetPairId, old =>
            {
                var newExchanges = old.Exchanges
                    .SetItems(old.Exchanges.Where(o => exchangesHashSet.Contains(o.Key))
                        .Select(s => KeyValuePair.Create(s.Key, changeFunc(s.Value))));
                return new AssetPairExtPriceSettings(exchanges: newExchanges, );
            }, reason);
        }

        private void CanPerformHedgingChanged(string assetPairId,
            ImmutableDictionary<string, ExchangeExtPriceSettings> oldSettings,
            ImmutableDictionary<string, ExchangeExtPriceSettings> newSettings, string reason)
        {
            var (hedgingOn, hedgingOff) = oldSettings.FindChanges(newSettings, e => e.Key,
                    CanPerformHedging,
                    (o, n) => o == n, ChangeType.Add, ChangeType.Update)
                .Select(t => t.New)
                .Partition(CanPerformHedging);

            ExchangesStateChanged(assetPairId, hedgingOn.Select(t => t.Key), "available for hedging", reason);
            ExchangesStateChanged(assetPairId, hedgingOff.Select(t => t.Key), "unavailable for hedging", reason);
        }

        private void ExchangesTemporarilyDisabledChanged(string assetPairId,
            ImmutableDictionary<string, ExchangeExtPriceSettings> oldSettings,
            ImmutableDictionary<string, ExchangeExtPriceSettings> newSettings, string reason)
        {
            var (disable, enable) = oldSettings.FindChanges(newSettings, e => e.Key,
                    e => e.Value.Disabled.IsTemporarilyDisabled,
                    (o, n) => o == n, ChangeType.Add, ChangeType.Update)
                .Select(t => t.New)
                .Partition(ex => ex.Value.Disabled.IsTemporarilyDisabled);
            ExchangesStateChanged(assetPairId, disable.Select(t => t.Key), "disabled", reason);
            ExchangesStateChanged(assetPairId, enable.Select(t => t.Key), "enabled", reason);
        }

        private static ImmutableDictionary<OrderbookGeneratorStepEnum, bool> ConvertSteps(ImmutableDictionary<OrderbookGeneratorStepEnum, bool> steps)
        {
            return GetDefaultSteps().SetItems(steps);
        }

        private static ImmutableDictionary<OrderbookGeneratorStepEnum, bool> GetDefaultSteps()
        {
            return Enum.GetValues(typeof(OrderbookGeneratorStepEnum)).Cast<OrderbookGeneratorStepEnum>()
                .ToImmutableDictionary(e => e, e => true);
        }
    }
}