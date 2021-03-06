﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class PrimaryExchangeService : IPrimaryExchangeService
    {
        private readonly ReadWriteLockedDictionary<string, string> _primaryExchanges =
            new ReadWriteLockedDictionary<string, string>();

        private readonly ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExchangeQuality>>
            _exchangesQualities =
                new ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExchangeQuality>>();

        private readonly IAlertService _alertService;
        private readonly IHedgingPreferenceService _hedgingPreferenceService;
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IStopTradesService _stopTradesService;
        private readonly IConvertService _convertService;

        public PrimaryExchangeService(IAlertService alertService, IHedgingPreferenceService hedgingPreferenceService,
            IExtPricesSettingsService extPricesSettingsService, IStopTradesService stopTradesService,
            IConvertService convertService)
        {
            _alertService = alertService;
            _hedgingPreferenceService = hedgingPreferenceService;
            _extPricesSettingsService = extPricesSettingsService;
            _stopTradesService = stopTradesService;
            _convertService = convertService;
        }

        public IReadOnlyDictionary<string, string> GetLastPrimaryExchanges()
        {
            return _primaryExchanges.ToImmutableDictionary();
        }

        public string GetLastPrimaryExchange(string assetPairId)
        {
            return _primaryExchanges.GetOrDefault(assetPairId);
        }

        public IReadOnlyDictionary<string, ImmutableDictionary<string, ExchangeQuality>> GetQualities()
        {
            return _exchangesQualities.ToImmutableDictionary();
        }

        public ImmutableDictionary<string, ExchangeQuality> GetQualities(string assetPairId)
        {
            return _exchangesQualities.GetValueOrDefault(assetPairId,
                ImmutableDictionary<string, ExchangeQuality>.Empty);
        }

        public ExchangeQuality GetPrimaryExchange(string assetPairId,
            ImmutableDictionary<string, ExchangeErrorStateDomainEnum> errors,
            DateTime now, string currentProcessingExchange)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.ChoosePrimary, assetPairId))
            {
                var presetPrimaryExchange = _extPricesSettingsService.GetPresetPrimaryExchange(assetPairId);
                _primaryExchanges[assetPairId] = presetPrimaryExchange;
                return new ExchangeQuality(presetPrimaryExchange, 0, ExchangeErrorStateDomainEnum.Valid, false, null);
            }

            var exchangeQualities = CalcExchangeQualities(assetPairId, errors, now, currentProcessingExchange);
            var primaryQuality = CheckPrimaryStatusAndSwitchIfNeeded(assetPairId, exchangeQualities);
            if (primaryQuality == null) return null;
            
            _stopTradesService.SetPrimaryOrderbookState(assetPairId, primaryQuality.ExchangeName, now,
                primaryQuality.HedgingPreference, primaryQuality.ErrorState);
            return primaryQuality;
        }

        [CanBeNull]
        private ExchangeQuality CheckPrimaryStatusAndSwitchIfNeeded(string assetPairId,
            ImmutableDictionary<string, ExchangeQuality> exchangeQualities)
        {
            var primaryExchange = _primaryExchanges.GetOrDefault(assetPairId);
            var originalPrimaryExchange = primaryExchange;

            var primaryQuality = primaryExchange == null ? null : exchangeQualities.GetValueOrDefault(primaryExchange);
            if (primaryQuality == null)
            {
                var result = SwitchPrimaryExchange(assetPairId, null, exchangeQualities,
                    newPrimary =>
                        $"{newPrimary.ExchangeName} has been chosen as an initial primary exchange for {assetPairId}. " +
                        $"It has error state \"{newPrimary.ErrorState}\" and hedging preference \"{newPrimary.HedgingPreference}\".");
                return result;
            }

            var primaryPreference = primaryQuality.HedgingPreference;
            var primaryError = primaryQuality.ErrorState;
            switch (primaryError)
            {
                case ExchangeErrorStateDomainEnum.Valid when primaryPreference > 0:
                    return primaryQuality;
                case ExchangeErrorStateDomainEnum.Outlier when primaryPreference > 0:
                    _alertService.AlertRiskOfficer(assetPairId,
                        $"Primary exchange {primaryExchange} for {assetPairId} is an outlier. Skipping price update.", EventTypeEnum.OutlierDetected);
                    return primaryQuality;
                case ExchangeErrorStateDomainEnum.Broken when primaryPreference > 0:
                    _alertService.AlertRiskOfficer(assetPairId,
                        $"Primary exchange {primaryExchange} for {assetPairId} is broken. Skipping price update.", EventTypeEnum.OutlierDetected);
                    return primaryQuality;
                default:
                    primaryQuality = SwitchPrimaryExchange(assetPairId, primaryQuality, exchangeQualities,
                        newPrimary =>
                            $"Primary exchange {originalPrimaryExchange} for {assetPairId} was changed.\r\n" +
                            $"It had error state \"{primaryError}\" and hedging preference \"{primaryPreference}\".\r\n" +
                            $"New primary exchange: \"{newPrimary.ExchangeName}\". It has error state \"{newPrimary.ErrorState}\" and hedging preference \"{newPrimary.HedgingPreference}\".");
                    return primaryQuality;
            }
        }

        private ExchangeQuality SwitchPrimaryExchange(string assetPairId, [CanBeNull] ExchangeQuality oldPrimary,
            ImmutableDictionary<string, ExchangeQuality> exchangeQualities,
            Func<ExchangeQuality, string> alertMessage)
        {
            var newPrimary = ChooseBackupExchange(assetPairId, exchangeQualities);
            if (newPrimary.ExchangeName == oldPrimary?.ExchangeName)
            {
                Trace.Write(TraceLevelGroupEnum.WarnTrace, assetPairId,
                    $"Current exchange {oldPrimary.ExchangeName} is bad, but switch failed",
                    new {Event = "ExchangeSwitchFailed", Reason = "No valid hedgable exchanges", oldPrimary.ExchangeName});
                return oldPrimary;
            }

            _primaryExchanges[assetPairId] = newPrimary.ExchangeName;
            _alertService.AlertPrimaryExchangeSwitched(
                new PrimaryExchangeSwitchedMessage
                {
                    AssetPairId = assetPairId,
                    AllExchangesStates = exchangeQualities.Values.Select(Convert).ToImmutableArray(),
                    NewPrimaryExchange = Convert(newPrimary),
                });
            _alertService.AlertRiskOfficer(assetPairId, alertMessage(newPrimary), EventTypeEnum.PrimaryExchangeChanged);
            return newPrimary;
        }

        private static string GetExchangesQualitiesString(IEnumerable<ExchangeQuality> exchangeQualities)
        {
            return string.Join(", ", exchangeQualities.OrderByDescending(q => q.HedgingPreference));
        }

        [Pure]
        private static ExchangeQuality ChooseBackupExchange(string assetPairId,
            IReadOnlyDictionary<string, ExchangeQuality> exchangeQualities)
        {
            var allHedgingPriorities = exchangeQualities
                .Values
                .Where(p => p.ErrorState != null && p.ErrorState != ExchangeErrorStateDomainEnum.Disabled)
                // ReSharper disable once PossibleInvalidOperationException
                .ToLookup(t => t.ErrorState.Value);

            var primary = allHedgingPriorities[ExchangeErrorStateDomainEnum.Valid]
                .OrderByDescending(p => p.HedgingPreference)
                .ThenBy(p => p.ExchangeName)
                .FirstOrDefault();

            if (primary != null && primary.HedgingPreference > 0)
            {
                return primary;
            }

            foreach (var state in new[] {ExchangeErrorStateDomainEnum.Valid, ExchangeErrorStateDomainEnum.Outlier})
            {
                primary = allHedgingPriorities[state].OrderByDescending(p => p.HedgingPreference)
                    .ThenBy(p => p.ExchangeName).FirstOrDefault();
                if (primary != null)
                {
                    return primary;
                }
            }

            throw new InvalidOperationException("Unable to choose backup exchange for assetPair " + assetPairId);
        }

        private ImmutableDictionary<string, ExchangeQuality> CalcExchangeQualities(string assetPairId,
            [NotNull] ImmutableDictionary<string, ExchangeErrorStateDomainEnum> errors, DateTime now,
            string currentProcessingExchange)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            var hedgingPreferences = _hedgingPreferenceService.Get(assetPairId);

            ImmutableDictionary<string, ExchangeQuality> Calc(ImmutableDictionary<string, ExchangeQuality> old)
                => hedgingPreferences.ToImmutableDictionary(p => p.Key,
                    p =>
                    {
                        var orderbookReceived = errors.TryGetValue(p.Key, out var state);
                        return new ExchangeQuality(p.Key, p.Value,
                            orderbookReceived ? state : (ExchangeErrorStateDomainEnum?) null,
                            orderbookReceived,
                            currentProcessingExchange == p.Key
                                ? now
                                : old?.GetValueOrDefault(p.Key).LastOrderbookReceivedTime);
                    });

            ImmutableDictionary<string, ExchangeQuality> oldQualities = null;
            var exchangeQualities = _exchangesQualities.AddOrUpdate(assetPairId, k => Calc(null),
                (k, old) =>
                {
                    oldQualities = old;
                    return Calc(old);
                });

            AlertIfQualitiesChanged(assetPairId, oldQualities, exchangeQualities);
            return exchangeQualities;
        }

        private void AlertIfQualitiesChanged(string assetPairId,
            [CanBeNull] ImmutableDictionary<string, ExchangeQuality> oldQualities,
            ImmutableDictionary<string, ExchangeQuality> exchangeQualities)
        {
            var hasChanges = oldQualities?.Values
                                 .FindChanges(exchangeQualities.Values, q => q.ExchangeName, q => q.ToString(),
                                     (o, n) => o == n).Any()
                             ?? true;
            if (hasChanges)
            {
                Task.Run(() =>
                {
                    var validHedgableCount = exchangeQualities.Values.Count(q =>
                        q.ErrorState == ExchangeErrorStateDomainEnum.Valid && q.HedgingPreference > 0);
                    var validCount = exchangeQualities.Values.Count(q =>
                        q.ErrorState == ExchangeErrorStateDomainEnum.Valid);
                    var activeCount = exchangeQualities.Values.Count(q =>
                        q.ErrorState == ExchangeErrorStateDomainEnum.Valid ||
                        q.ErrorState == ExchangeErrorStateDomainEnum.Outlier);
                    _alertService.AlertRiskOfficer(assetPairId,
                        $"{assetPairId}: now {validHedgableCount} valid & available for hedging, " +
                        $"{validCount} valid, {activeCount} active, {exchangeQualities.Count} configured exchanges: \r\n" +
                        GetExchangesQualitiesString(exchangeQualities.Values), EventTypeEnum.StatusInfo);
                });
            }
        }

        [Pure]
        private ExchangeQualityMessage Convert(ExchangeQuality quality)
        {
            return _convertService.Convert<ExchangeQuality, ExchangeQualityMessage>(quality);
        }
    }
}