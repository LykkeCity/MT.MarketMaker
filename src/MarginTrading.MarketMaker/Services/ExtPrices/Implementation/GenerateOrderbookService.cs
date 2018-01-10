using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Log;
using JetBrains.Annotations;
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
    public class GenerateOrderbookService : ICustomStartup, IDisposable, IGenerateOrderbookService
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

            var (result, primaryExchange, problem) = OnNewOrderbookInternal(orderbook);
            LogCycle(orderbook, result, watch, primaryExchange, problem);
            return result;
        }

        private (Orderbook Orderbook, ExchangeQuality PrimaryExchange, string Problem) OnNewOrderbookInternal(
            ExternalOrderbook orderbook)
        {
            var assetPairId = orderbook.AssetPairId;
            if (!_extPricesSettingsService.IsExchangeConfigured(assetPairId, orderbook.ExchangeName))
            {
                Trace.Write(TraceLevelGroupEnum.WarnTrace, assetPairId,
                    $"Skipping not configured exchange {orderbook.ExchangeName}",
                    new {Event = "NotConfiguredExchangeSkipped", orderbook.ExchangeName});
                return (null, null, "Skipping not configured exchange");
            }

            var allOrderbooks = _orderbooksService.AddAndGetByAssetPair(orderbook);
            var now = orderbook.LastUpdatedTime;
            var (exchangesErrors, validOrderbooks) = MarkExchangesErrors(assetPairId, allOrderbooks, now);
            var primaryExchangeQuality =
                _primaryExchangeService.GetPrimaryExchange(assetPairId, exchangesErrors, now, orderbook.ExchangeName);
            if (primaryExchangeQuality == null)
            {
                return (null, null, "No primary exchange");
            }

            var primaryExchangeName = primaryExchangeQuality.ExchangeName;
            if (primaryExchangeName != orderbook.ExchangeName)
            {
                return (null, primaryExchangeQuality, "Orderbook not from primary exchange");
            }

            if (primaryExchangeQuality.ErrorState == ExchangeErrorStateDomainEnum.Outlier)
            {
                return (null, primaryExchangeQuality, "Primary exchange is an outlier, skipping price update");
            }

            if (!allOrderbooks.TryGetValue(primaryExchangeName, out var primaryOrderbook))
            {
                _log.WriteErrorAsync(nameof(GenerateOrderbookService), null,
                    new Exception($"{primaryExchangeName} not found in allOrderbooks ({allOrderbooks.Keys.ToJson()})")
                    {
                        Data = {{"AssetPairId", assetPairId}}
                    });
                return (null, primaryExchangeQuality, "Primary exchange orderbook not found");
            }

            _stopTradesService.FinishCycle(primaryOrderbook, now);
            var resultingOrderbook = Transform(primaryOrderbook, validOrderbooks);
            if (TryFindSkipOrderbookReason(resultingOrderbook) is string reason)
            {
                return (null, primaryExchangeQuality, reason);
            }

            return (resultingOrderbook, primaryExchangeQuality, null);
        }

        [CanBeNull]
        private string TryFindSkipOrderbookReason(Orderbook orderbook)
        {
            var now = _system.UtcNow;
            string reason = null;
            var period = _extPricesSettingsService.GetMinOrderbooksSendingPeriod(orderbook.AssetPairId);
            _sentOrderbooks.AddOrUpdate(orderbook.AssetPairId, k => now,
                (k, lastTime) =>
                {
                    if (now.Subtract(lastTime) < period)
                    {
                        reason = "Too frequient update";
                        return lastTime;
                    }
                    else
                    {
                        return now;
                    }
                });
            return reason;
        }

        public void Initialize()
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
        private (ImmutableDictionary<string, ExchangeErrorStateDomainEnum>,
            ImmutableDictionary<string, ExternalOrderbook>)
            MarkExchangesErrors(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> allOrderbooks,
                DateTime now)
        {
            var disabledExchanges = _disabledOrderbooksService.GetDisabledExchanges(assetPairId);
            var enabledOrderbooks = allOrderbooks.RemoveRange(disabledExchanges);
            var (brokenExchanges, nonBrokenExchanges) = FindBroken(assetPairId, enabledOrderbooks);
            var (outdatedExchanges, freshOrderbooks) = FindOutdated(assetPairId, nonBrokenExchanges, now);
            var (outliersExchanges, validOrderbooks) = FindOutliers(assetPairId, freshOrderbooks, now);

            var repeatedProblemsExchanges = GetRepeatedProblemsExchanges(assetPairId, enabledOrderbooks,
                outdatedExchanges, outliersExchanges, now);
            _disabledOrderbooksService.Disable(assetPairId, repeatedProblemsExchanges, "Repeated outlier");

            var exchangesErrors = ImmutableDictionary.CreateBuilder<string, ExchangeErrorStateDomainEnum>()
                .SetValueForKeys(disabledExchanges, ExchangeErrorStateDomainEnum.Disabled)
                .SetValueForKeys(brokenExchanges, ExchangeErrorStateDomainEnum.Broken)
                .SetValueForKeys(outdatedExchanges, ExchangeErrorStateDomainEnum.Outdated)
                .SetValueForKeys(outliersExchanges, ExchangeErrorStateDomainEnum.Outlier)
                .SetValueForKeys(validOrderbooks.Keys, ExchangeErrorStateDomainEnum.Valid)
                .SetValueForKeys(repeatedProblemsExchanges, ExchangeErrorStateDomainEnum.Disabled)
                .ToImmutable();

            return (exchangesErrors, validOrderbooks);
        }

        private (ImmutableHashSet<string>, ImmutableDictionary<string, ExternalOrderbook>) FindBroken(
            string assetPairId, ImmutableDictionary<string, ExternalOrderbook> enabledOrderbooks)
        {
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.FindBroken, assetPairId))
            {
                return (ImmutableHashSet<string>.Empty, enabledOrderbooks);
            }

            // 1.7*bid > ask > bid > 0 (see LWDEV-4587)
            var brokenExchanges = enabledOrderbooks.Values
                .Where(o =>
                {
                    var bestPrices = _bestPricesService.Calc(o);
                    return bestPrices.BestBid <= 0 ||
                           bestPrices.BestAsk < bestPrices.BestBid ||
                           5m/3m * bestPrices.BestBid <= bestPrices.BestAsk;
                })
                .Select(o => o.ExchangeName)
                .ToImmutableHashSet();
            var nonBrokenExchanges = enabledOrderbooks.RemoveRange(brokenExchanges);
            return (brokenExchanges, nonBrokenExchanges);
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
            if (!_extPricesSettingsService.IsStepEnabled(OrderbookGeneratorStepDomainEnum.FindRepeatedProblems,
                assetPairId))
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

            var outliersExchanges = _outliersOrderbooksService.FindOutliers(assetPairId, freshOrderbooks)
                .Select(o => o.ExchangeName)
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

        private void LogCycle(ExternalOrderbook orderbook, [CanBeNull] Orderbook resultingOrderbook, Stopwatch watch,
            ExchangeQuality primaryExchangeQuality, [CanBeNull] string problem)
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
                        {"IsPrimary", (orderbook.ExchangeName == primaryExchangeQuality.ExchangeName).ToString()},
                        {"IsSkip", (resultingOrderbook == null).ToString()},
                    });
            }

            var bestPrices = _bestPricesService.Calc(orderbook);
            var resultingBestPrices = resultingOrderbook == null ? null : _bestPricesService.Calc(resultingOrderbook);
            var action = resultingOrderbook == null ? "Skipped" : "Processed";
            Trace.Write(TraceLevelGroupEnum.Trace, orderbook.AssetPairId,
                $"{action} from {orderbook.ExchangeName}, " +
                $"primary: {primaryExchangeQuality}, time: {elapsedMilliseconds} ms",
                new
                {
                    Event = "ExternalOrderbook" + action,
                    Reason = problem,
                    orderbook.ExchangeName,
                    PrimaryExchange = primaryExchangeQuality,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    IsSkip = resultingOrderbook == null,
                    bestPrices.BestBid,
                    bestPrices.BestAsk,
                    ResultingBestBid = resultingBestPrices?.BestBid,
                    ResultingBestAsk = resultingBestPrices?.BestAsk,
                    BidsDepth = orderbook.Bids.Length,
                    AsksDepth = orderbook.Asks.Length,
                    ResultingsBidsDepth = resultingOrderbook?.Bids.Length,
                    ResultingsAsksDepth = resultingOrderbook?.Asks.Length,
                });
        }
    }
}