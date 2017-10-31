﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implemetation;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    public class PrimaryExchangeService : IPrimaryExchangeService
    {
        private readonly ReadWriteLockedDictionary<string, string> _primaryExchanges =
            new ReadWriteLockedDictionary<string, string>();
        private readonly ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExchangeQuality>> _exchangesQualities =
            new ReadWriteLockedDictionary<string, ImmutableDictionary<string, ExchangeQuality>>();

        private readonly IAlertService _alertService;
        private readonly IHedgingPreferenceService _hedgingPreferenceService;
        private readonly IPriceCalcSettingsService _priceCalcSettingsService;
        private readonly IStopTradesService _stopTradesService;

        public PrimaryExchangeService(IAlertService alertService, IHedgingPreferenceService hedgingPreferenceService,
            IPriceCalcSettingsService priceCalcSettingsService, IStopTradesService stopTradesService)
        {
            _alertService = alertService;
            _hedgingPreferenceService = hedgingPreferenceService;
            _priceCalcSettingsService = priceCalcSettingsService;
            _stopTradesService = stopTradesService;
        }

        [Pure]
        public IReadOnlyDictionary<string, string> GetLastPrimaryExchanges()
        {
            return _primaryExchanges.ToImmutableDictionary();
        }

        [Pure]
        public string GetLastPrimaryExchange(string assetPairId)
        {
            return _primaryExchanges.GetOrDefault(assetPairId);
        }

        [Pure]
        public IReadOnlyDictionary<string, ImmutableDictionary<string, ExchangeQuality>> GetQualities()
        {
            return _exchangesQualities.ToImmutableDictionary();
        }

        public string GetPrimaryExchange(string assetPairId, ImmutableDictionary<string, ExchangeErrorState> errors,
            DateTime now)
        {
            if (!_priceCalcSettingsService.IsStepEnabled(OrderbookGeneratorStepEnum.ChoosePrimary, assetPairId))
            {
                var presetPrimaryExchange = _priceCalcSettingsService.GetPresetPrimaryExchange(assetPairId);
                _primaryExchanges[assetPairId] = presetPrimaryExchange;
                return presetPrimaryExchange;
            }

            var exchangeQualities = CalcExchangeQualities(assetPairId, errors);
            var primaryQuality = CheckPrimaryStatusAndSwitchIfNeeded(assetPairId, exchangeQualities);
            _stopTradesService.SetPrimaryOrderbookState(assetPairId, primaryQuality.Exchange, now,
                primaryQuality.HedgingPreference, primaryQuality.Error);
            return primaryQuality.Error == ExchangeErrorState.Outlier ? null : primaryQuality.Exchange;
        }

        private ExchangeQuality CheckPrimaryStatusAndSwitchIfNeeded(string assetPairId,
            ImmutableDictionary<string, ExchangeQuality> exchangeQualities)
        {
            var primaryExchange = _primaryExchanges.GetOrDefault(assetPairId);
            var originalPrimaryExchange = primaryExchange;

            if (primaryExchange == null)
            {
                var result = SwitchPrimaryExchange(assetPairId, null, exchangeQualities,
                    newPrimary => $"{newPrimary.Exchange} has been chosen as an initial primary exchange for {assetPairId}. " +
                                  $"It has error state \"{newPrimary.Error}\" and hedging preference \"{newPrimary.HedgingPreference}\".");
                return result;
            }

            var primaryQuality = exchangeQualities.GetValueOrDefault(primaryExchange);
            var primaryPreference = primaryQuality.HedgingPreference;
            var primaryError = primaryQuality.Error;
            switch (primaryError)
            {
                case ExchangeErrorState.None when primaryPreference > 0:
                    return primaryQuality;
                case ExchangeErrorState.Outlier when primaryPreference > 0:
                    _alertService.AlertRiskOfficer(
                        $"Primary exchange {primaryExchange} for {assetPairId} is an outlier. Skipping price update.");
                    return primaryQuality;
                default:
                    primaryQuality = SwitchPrimaryExchange(assetPairId, primaryQuality, exchangeQualities,
                        newPrimary =>
                            $"Primary exchange {originalPrimaryExchange} for {assetPairId} was changed.\r\n" +
                            $"It had error state \"{primaryError}\" and hedging preference \"{primaryPreference}\".\r\n" +
                            $"New primary exchange: \"{newPrimary.Exchange}\". It has error state \"{newPrimary.Error}\" and hedging preference \"{newPrimary.HedgingPreference}\".");
                    return primaryQuality;
            }
        }

        private ExchangeQuality SwitchPrimaryExchange(string assetPairId, [CanBeNull] ExchangeQuality oldPrimary,
            ImmutableDictionary<string, ExchangeQuality> exchangeQualities,
            Func<ExchangeQuality, string> alertMessage)
        {
            var newPrimary = ChooseBackupExchange(assetPairId, exchangeQualities);
            if (newPrimary.Exchange == oldPrimary?.Exchange)
            {
                var exchanges = string.Join(", ", exchangeQualities.OrderByDescending(q=>q.Value.HedgingPreference).Select(q => q.Value.ToString()));
                Trace.Write($"Current exchange {oldPrimary.Exchange} for {assetPairId} is bad, but switch failed. Exchanges: {exchanges}");
                return oldPrimary;
            }

            _primaryExchanges[assetPairId] = newPrimary.Exchange;
            _alertService.AlertPrimaryExchangeSwitched(
                new PrimaryExchangeSwitchedMessage
                {
                    AssetPairId = assetPairId,
                    AllExchangesStates = exchangeQualities.Values.Select(Convert).ToImmutableArray(),
                    NewPrimaryExchange = Convert(newPrimary),
                });
            _alertService.AlertRiskOfficer(alertMessage(newPrimary));
            return newPrimary;
        }

        [Pure]
        private static ExchangeQuality ChooseBackupExchange(string assetPairId,
            IReadOnlyDictionary<string, ExchangeQuality> exchangeQualities)
        {
            var allHedgingPriorities = exchangeQualities
                .Values
                .Where(p => p.Error != null && p.Error != ExchangeErrorState.Disabled)
                // ReSharper disable once PossibleInvalidOperationException
                .ToLookup(t => t.Error.Value);

            var primary = allHedgingPriorities[ExchangeErrorState.None]
                .OrderByDescending(p => p.HedgingPreference)
                .FirstOrDefault();

            if (primary != null && primary.HedgingPreference > 0)
            {
                return primary;
            }

            foreach (var state in new[] {ExchangeErrorState.None, ExchangeErrorState.Outlier})
            {
                primary = allHedgingPriorities[state].OrderByDescending(p => p.HedgingPreference).FirstOrDefault();
                if (primary != null)
                {
                    return primary;
                }
            }

            throw new InvalidOperationException("Unable to choose backup exchange for assetPair " + assetPairId);
        }

        private ImmutableDictionary<string, ExchangeQuality> CalcExchangeQualities(string assetPairId,
            ImmutableDictionary<string, ExchangeErrorState> errors)
        {
            var exchangeQualities = _hedgingPreferenceService.Get(assetPairId)
                .ToImmutableDictionary(p => p.Key,
                    p =>
                    {
                        var orderbookReceived = errors.TryGetValue(p.Key, out var state);
                        return new ExchangeQuality(p.Key, p.Value,
                            orderbookReceived ? state : (ExchangeErrorState?) null,
                            orderbookReceived);
                    });

            _exchangesQualities[assetPairId] = exchangeQualities;
            return exchangeQualities;
        }

        [Pure]
        private static ExchangeQualityMessage Convert(ExchangeQuality quality)
        {
            return new ExchangeQualityMessage
            {
                Error = quality.Error,
                Exchange = quality.Exchange,
                HedgingPreference = quality.HedgingPreference,
                OrderbookReceived = quality.OrderbookReceived,
            };
        }
    }
}