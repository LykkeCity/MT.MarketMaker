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
            return (decimal) GetExchange(assetPairId, exchangeName).OrderGeneration.VolumeMultiplier;
        }

        public TimeSpan GetOrderbookOutdatingThreshold(string assetPairId, string exchangeName, DateTime now)
        {
            return GetExchange(assetPairId, exchangeName).OrderbookOutdatingThreshold;
        }

        public RepeatedOutliersParams GetRepeatedOutliersParams(string assetPairId)
        {
            var p = GetAsset(assetPairId).RepeatedOutliers;
            return new RepeatedOutliersParams(p.MaxSequenceLength, p.MaxSequenceAge, (decimal) p.MaxAvg, p.MaxAvgAge);
        }

        public decimal GetOutlierThreshold(string assetPairId)
        {
            return (decimal) GetAsset(assetPairId).OutlierThreshold;
        }

        public ImmutableDictionary<string, decimal> GetHedgingPreferences(string assetPairId)
        {
            return GetAllExchanges(assetPairId).ToImmutableDictionary(e => e.Key,
                e => e.Value.Hedging.IsTemporarilyUnavailable ? 0m : (decimal) e.Value.Hedging.DefaultPreference);
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

        public async Task ChangeExchangesTemporarilyDisabled(string assetPairId, ImmutableHashSet<string> exchanges, bool disable, string reason)
        {
            if (exchanges.Count == 0)
                return;

            await _exchangeExtPriceSettingsService.Update(assetPairId, exchanges,
                old => new ExchangeExtPriceSettings(, new ExchangeExtPriceSettings.DisabledSettings(disable, reason), ));
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

        public bool IsExchangeConfigured(string assetPairId, string exchange)
        {
            return GetAllExchanges(assetPairId).GetValueOrDefault(exchange) != null;
        }

        public TimeSpan GetMinOrderbooksSendingPeriod(string assetPairId)
        {
            return GetAsset(assetPairId).MinOrderbooksSendingPeriod;
        }

        public Task AddAsync(AssetPairExtPriceSettings setting)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(AssetPairExtPriceSettings setting)
        {
            throw new NotImplementedException();
        }

        public ImmutableDictionary<string, AssetPairExtPriceSettings> Get()
        {
            throw new NotImplementedException();
        }

        AssetPairExtPriceSettings IExtPricesSettingsService.Get(string assetPairId)
        {
            throw new NotImplementedException();
        }

        public async AssetPairExtPriceSettingsModel Get(string assetPairId = null)
        {
            IEnumerable<AssetPairExtPriceSettingsEntity> assetPairsEntities;
            if (assetPairId == null)
                assetPairsEntities = await _assetPairsRepository.GetAllAsync();
            else
                assetPairsEntities =
                    Enumerable.Where<AssetPairExtPriceSettingsEntity>(new[] {_assetPairsCachedAccessor.GetByKey(GetAssetPairKeys(assetPairId))}, e => e != null);

            return assetPairsEntities
                .GroupJoin(await _exchangesRepository.GetAllAsync(), ap => ap.AssetPairId, e => e.AssetPairId, Convert)
                .ToList();
        }

        public Task Set(AssetPairExtPriceSettingsModel model)
        {
            var entity = Convert(model, TryGetAsset(model.AssetPairId));
            entity.Timestamp = DateTimeOffset.UtcNow;

            var upsertAssetPairTask = _assetPairsCachedAccessor.Upsert(entity);

            var exchangesEntities = model.Exchanges.Select(e => Convert(e, entity)).ToImmutableDictionary(e => e.Exchange);
            ImmutableDictionary<string, ExchangeExtPriceSettings> oldExchangesEntities = ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty;

            // do not update existing entities instances!
            _exchangesCache.AddOrUpdate(entity.AssetPairId,
                k =>
                {
                    _exchangesRepository.InsertOrReplaceAsync(exchangesEntities.Values).GetAwaiter().GetResult();
                    return exchangesEntities;
                },
                (k, old) =>
                {
                    oldExchangesEntities = old;
                    Task.WaitAll(
                        _exchangesRepository.InsertOrReplaceAsync(exchangesEntities.Values),
                        _exchangesRepository.DeleteAsync(old.Values.Where(o =>
                            !exchangesEntities.ContainsKey(o.Exchange))));
                    return exchangesEntities;
                });

            const string reason = "settings was manually changed";
            return upsertAssetPairTask;
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

        private ExchangeExtPriceSettings GetExchange(string assetPairId, string exchange)
        {
            return GetAllExchanges(assetPairId).GetValueOrDefault(exchange)
                   ?? throw new ArgumentException(
                       $"Settings for exchange {exchange} for asset pair {assetPairId} not found", nameof(exchange));
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

        private void UpdateExchanges(string assetPairId, IEnumerable<string> exchangeNames,
            Func<ExchangeExtPriceSettings, ExchangeExtPriceSettings> changeFunc, string reason)
        {
            var oldExchanges;
            var newExchanges;
            return _settingsRootService.Update(assetPairId, old =>
            {
                oldExchanges = old.ExtPriceSettings
            })
            ExchangesTemporarilyDisabledChanged(entity.AssetPairId, oldExchanges, newExchanges, reason);
            CanPerformHedgingChanged(entity.AssetPairId, oldExchanges, newExchanges, reason);
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
            return Enum.GetValues(typeof(OrderbookGeneratorStepEnum)).Cast<OrderbookGeneratorStepEnum>()
                .ToImmutableDictionary(e => e, e => steps.GetValueOrDefault(e, true));
        }
    }
}