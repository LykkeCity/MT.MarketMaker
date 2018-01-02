using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;
using Trace = MarginTrading.MarketMaker.Infrastructure.Implementation.Trace;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    /// <summary>
    ///     Generates orderbooks from external exchanges
    /// </summary>
    /// <remarks>
    ///     https://lykkex.atlassian.net/wiki/spaces/MW/pages/84607035/Price+setting
    /// </remarks>
    public class GenerateOrderbookService : IStartable, IDisposable, IGenerateOrderbookService
    {
        private readonly ReadWriteLockedDictionary<string, DateTime> _sentOrderbooks =
            new ReadWriteLockedDictionary<string, DateTime>();

        private readonly IOrderbooksService _orderbooksService;
        private readonly IDisabledOrderbooksService _disabledOrderbooksService;
        private readonly IOutdatedOrderbooksService _outdatedOrderbooksService;
        private readonly IOutliersOrderbooksService _outliersOrderbooksService;
        private readonly IRepeatedProblemsOrderbooksService _repeatedProblemsOrderbooksService;
        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IAlertService _alertService;
        private readonly IPrimaryExchangeService _primaryExchangeService;
        private readonly ITransformOrderbookService _transformOrderbookService;
        private readonly IBestPricesService _bestPricesService;
        private readonly ILog _log;
        private readonly ITelemetryService _telemetryService;
        private readonly ITestingHelperService _testingHelperService;
        private readonly IStopTradesService _stopTradesService;
        private readonly ISystem _system;


        public GenerateOrderbookService(
            IOrderbooksService orderbooksService,
            IDisabledOrderbooksService disabledOrderbooksService,
            IOutdatedOrderbooksService outdatedOrderbooksService,
            IOutliersOrderbooksService outliersOrderbooksService,
            IRepeatedProblemsOrderbooksService repeatedProblemsOrderbooksService,
            IExtPricesSettingsService extPricesSettingsService,
            IAlertService alertService,
            IPrimaryExchangeService primaryExchangeService,
            ITransformOrderbookService transformOrderbookService,
            IBestPricesService bestPricesService,
            ILog log,
            ITelemetryService telemetryService,
            ITestingHelperService testingHelperService,
            IStopTradesService stopTradesService, ISystem system)
        {
            _orderbooksService = orderbooksService;
            _disabledOrderbooksService = disabledOrderbooksService;
            _outdatedOrderbooksService = outdatedOrderbooksService;
            _outliersOrderbooksService = outliersOrderbooksService;
            _repeatedProblemsOrderbooksService = repeatedProblemsOrderbooksService;
            _extPricesSettingsService = extPricesSettingsService;
            _alertService = alertService;
            _primaryExchangeService = primaryExchangeService;
            _transformOrderbookService = transformOrderbookService;
            _bestPricesService = bestPricesService;
            _log = log;
            _telemetryService = telemetryService;
            _testingHelperService = testingHelperService;
            _stopTradesService = stopTradesService;
            _system = system;
        }

        public Orderbook OnNewOrderbook(ExternalOrderbook orderbook)
        {
            if (orderbook == null) throw new ArgumentNullException(nameof(orderbook));
            var watch = Stopwatch.StartNew();
            orderbook = _testingHelperService.ModifyOrderbookIfNeeded(orderbook);
            if (orderbook == null)
            {
                return null;
            }

            var (result, primaryExchange) = OnNewOrderbookInternal(orderbook);
            LogCycle(orderbook, watch, primaryExchange, result == null);
            return result;
        }

        private (Orderbook Orderbook, string PrimaryExchange) OnNewOrderbookInternal(ExternalOrderbook orderbook)
        {
            var assetPairId = orderbook.AssetPairId;
            if (!_extPricesSettingsService.IsExchangeConfigured(assetPairId, orderbook.ExchangeName))
            {
                Trace.Write(assetPairId + " warn trace", $"Skipping not configured exchange {orderbook.ExchangeName} for {assetPairId}");
                return (null, null);
            }

            var allOrderbooks = _orderbooksService.AddAndGetByAssetPair(orderbook);
            var now = orderbook.LastUpdatedTime;
            var (exchangesErrors, validOrderbooks) = MarkExchangesErrors(assetPairId, allOrderbooks, now);
            var primaryExchange = _primaryExchangeService.GetPrimaryExchange(assetPairId, exchangesErrors, now, orderbook.ExchangeName);
            if (primaryExchange == null)
            {
                return (null, null);
            }

            if (primaryExchange != orderbook.ExchangeName)
            {
                return (null, primaryExchange);
            }

            if (!allOrderbooks.TryGetValue(primaryExchange, out var primaryOrderbook))
            {
                _log.WriteWarningAsync(nameof(GenerateOrderbookService), null,
                    $"{primaryExchange} not found in allOrderbooks ({allOrderbooks.Keys.ToJson()})");
                return (null, primaryExchange);
            }

            _stopTradesService.FinishCycle(primaryOrderbook, now);
            var resultingOrderbook = Transform(primaryOrderbook, validOrderbooks);
            if (!CanSendOrderbook(resultingOrderbook))
            {
                return (null, primaryExchange);
            }

            return (resultingOrderbook, primaryExchange);
        }

        private bool CanSendOrderbook(Orderbook orderbook)
        {
            var now = _system.UtcNow;
            bool canSend = true;
            var period = _extPricesSettingsService.GetMinOrderbooksSendingPeriod(orderbook.AssetPairId);
             _sentOrderbooks.AddOrUpdate(orderbook.AssetPairId, k => now,
                       (k, lastTime) =>
                       {
                           if (now.Subtract(lastTime) < period)
                           {
                               canSend = false;
                               return lastTime;
                           }
                           else
                           {
                               return now;
                           }
                       });
            return canSend;
        }

        public void Start()
        {
            _alertService.AlertStarted();
        }

        public void Dispose()
        {
            _alertService.AlertStopping().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Detects exchanges errors and disables thm if they get repeated
        /// </summary>
        private (ImmutableDictionary<string, ExchangeErrorStateDomainEnum>, ImmutableDictionary<string, ExternalOrderbook>)
            MarkExchangesErrors(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> allOrderbooks, DateTime now)
        {
            var disabledExchanges = _disabledOrderbooksService.GetDisabledExchanges(assetPairId);
            var enabledOrderbooks = allOrderbooks.RemoveRange(disabledExchanges);
            var (outdatedExchanges, freshOrderbooks) = FindOutdated(assetPairId, enabledOrderbooks, now);
            var (outliersExchanges, validOrderbooks) = FindOutliers(assetPairId, freshOrderbooks, now);

            var repeatedProblemsExchanges = GetRepeatedProblemsExchanges(assetPairId, enabledOrderbooks,
                outdatedExchanges, outliersExchanges, now);
            _disabledOrderbooksService.Disable(assetPairId, repeatedProblemsExchanges, "Repeated outlier");

            var exchangesErrors = ImmutableDictionary.CreateBuilder<string, ExchangeErrorStateDomainEnum>()
                .SetValueForKeys(disabledExchanges, ExchangeErrorStateDomainEnum.Disabled)
                .SetValueForKeys(outdatedExchanges, ExchangeErrorStateDomainEnum.Outdated)
                .SetValueForKeys(outliersExchanges, ExchangeErrorStateDomainEnum.Outlier)
                .SetValueForKeys(validOrderbooks.Keys, ExchangeErrorStateDomainEnum.Valid)
                .SetValueForKeys(repeatedProblemsExchanges, ExchangeErrorStateDomainEnum.Disabled)
                .ToImmutable();

            return (exchangesErrors, validOrderbooks);
        }

        /// <summary>
        ///     Applies arbitrage-free spread to the orderbook
        /// </summary>
        [CanBeNull]
        private Orderbook Transform(
            ExternalOrderbook primaryOrderbook,
            ImmutableDictionary<string, ExternalOrderbook> validOrderbooks)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.Transform,
                primaryOrderbook.AssetPairId))
            {
                return primaryOrderbook;
            }

            var bestPrices =
                validOrderbooks.Values.ToDictionary(o => o.ExchangeName,
                    orderbook => _bestPricesService.CalcExternal(orderbook));
            return _transformOrderbookService.Transform(primaryOrderbook, bestPrices);
        }

        /// <summary>
        ///     Detects exchanges with repeated problems
        /// </summary>
        private ImmutableHashSet<string> GetRepeatedProblemsExchanges(string assetPairId,
            ImmutableDictionary<string, ExternalOrderbook> orderbooksByExchanges,
            ImmutableHashSet<string> outdatedExchanges, ImmutableHashSet<string> outliersExchanges,
            DateTime now)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.FindRepeatedProblems, assetPairId))
            {
                return ImmutableHashSet<string>.Empty;
            }

            return orderbooksByExchanges.Values
                .Where(o => _repeatedProblemsOrderbooksService.IsRepeatedProblemsOrderbook(o,
                    outdatedExchanges.Contains(o.ExchangeName), outliersExchanges.Contains(o.ExchangeName), now))
                .Select(o => o.ExchangeName).ToImmutableHashSet();
        }


        /// <summary>
        ///     Detects outlier exchanges
        /// </summary>
        private (ImmutableHashSet<string>, ImmutableDictionary<string, ExternalOrderbook>) FindOutliers(
            string assetPairId, ImmutableDictionary<string, ExternalOrderbook> freshOrderbooks, DateTime now)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.FindOutliers, assetPairId))
            {
                return (ImmutableHashSet<string>.Empty, freshOrderbooks);
            }

            _stopTradesService.SetFreshOrderbooksState(assetPairId, freshOrderbooks, now);
            if (freshOrderbooks.Count < 3)
            {
                return (ImmutableHashSet<string>.Empty, freshOrderbooks);
            }

            var outliersExchanges = _outliersOrderbooksService.FindOutliers(assetPairId, freshOrderbooks).Select(o => o.ExchangeName)
                .ToImmutableHashSet();
            var freshNotOutlierOrderbooks = freshOrderbooks.RemoveRange(outliersExchanges);
            return (outliersExchanges, freshNotOutlierOrderbooks);
        }


        /// <summary>
        ///     Detects outdated exchanges
        /// </summary>
        private (ImmutableHashSet<string>, ImmutableDictionary<string, ExternalOrderbook>)
            FindOutdated(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> orderbooksByExchanges,
                DateTime now)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.FindOutdated, assetPairId))
            {
                return (ImmutableHashSet<string>.Empty, orderbooksByExchanges);
            }

            var outdatedExchanges = orderbooksByExchanges.Values
                .Where(o => _outdatedOrderbooksService.IsOutdated(o, now)).Select(o => o.ExchangeName)
                .ToImmutableHashSet();
            var freshOrderbooks = orderbooksByExchanges.RemoveRange(outdatedExchanges);
            return (outdatedExchanges, freshOrderbooks);
        }

        private void LogCycle(ExternalOrderbook orderbook, Stopwatch watch, string primaryExchange, bool isSkip)
        {
            var elapsedMilliseconds = watch.ElapsedMilliseconds;
            if (elapsedMilliseconds > 20)
            {
                _telemetryService.PublishEventMetrics(nameof(GenerateOrderbookService) + '.' + nameof(OnNewOrderbook),
                    null,
                    new Dictionary<string, double> {{"ProcessingTime", elapsedMilliseconds}},
                    new Dictionary<string, string>
                    {
                        {"AssetPairId", orderbook.AssetPairId},
                        {"Exchange", orderbook.ExchangeName},
                        {"IsPrimary", (orderbook.ExchangeName == primaryExchange).ToString()},
                        {"IsSkip", isSkip.ToString()},
                    });
            }

            Trace.Write(orderbook.AssetPairId + " trace",
                $"{(isSkip ? "Skipped " : "Processed")} {orderbook.AssetPairId} from {orderbook.ExchangeName}, primary: {primaryExchange}, time: {elapsedMilliseconds} ms");
        }
    }
}