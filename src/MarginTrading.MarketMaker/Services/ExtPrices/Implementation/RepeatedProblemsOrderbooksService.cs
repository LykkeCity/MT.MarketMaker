using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class RepeatedProblemsOrderbooksService : IRepeatedProblemsOrderbooksService
    {
        private readonly ReadWriteLockedDictionary<(string AssetPairId, string Exchange), ImmutableSortedSet<Event>>
            _lastEvents = new ReadWriteLockedDictionary<(string, string), ImmutableSortedSet<Event>>();

        private readonly IExtPricesSettingsService _extPricesSettingsService;
        private readonly IAlertService _alertService;

        public RepeatedProblemsOrderbooksService(IExtPricesSettingsService extPricesSettingsService,
            IAlertService alertService)
        {
            _extPricesSettingsService = extPricesSettingsService;
            _alertService = alertService;
        }

        public bool IsRepeatedProblemsOrderbook(ExternalOrderbook orderbook, bool isOutdated, bool isOutlier,
            DateTime now)
        {
            var assetPairId = orderbook.AssetPairId;
            var repeatedOutliersParams = _extPricesSettingsService.GetRepeatedOutliersParams(assetPairId);
            var outlierSequenceStart = now - repeatedOutliersParams.MaxSequenceAge;
            var outlierAvgStart = now - repeatedOutliersParams.MaxAvgAge;
            var minEventTime = outlierSequenceStart < outlierAvgStart ? outlierSequenceStart : outlierAvgStart;
            var newEvent = new Event(now, isOutdated, isOutlier);
            var actualProblems = _lastEvents.AddOrUpdate((assetPairId, orderbook.ExchangeName),
                k => ImmutableSortedSet.Create(Event.ComparerByTime, newEvent),
                (k, old) => AddEventAndCleanOld(old, newEvent, minEventTime));

            //currently we process only Outlier
            if (isOutlier)
            {
                var outliersInRow = 0;
                var statsCount = 0;
                var outliersCount = 0;
                foreach (var e in actualProblems)
                {
                    if (e.IsOutlier && e.Time >= outlierSequenceStart)
                        outliersInRow++;
                    else
                        outliersInRow = 0;

                    if (outliersInRow > repeatedOutliersParams.MaxSequenceLength)
                    {
                        _alertService.AlertRiskOfficer(assetPairId,
                            $"{orderbook.ExchangeName} is a repeated outlier exchange for {assetPairId}.\r\n" +
                            $"It had {outliersInRow} outlier orderbooks in a row " +
                            $"during last {repeatedOutliersParams.MaxSequenceAge.TotalSeconds:f0} secs.");
                        Trace.Write(TraceLevelGroupEnum.ErrorTrace, assetPairId, "Repeated outlier (sequence)",
                            new
                            {
                                Event = "Repeated outlier",
                                Reason = "Sequence",
                                orderbook.ExchangeName,
                                outliersInRow,
                                repeatedOutliersParams.MaxSequenceLength,
                                repeatedOutliersParams.MaxSequenceAge
                            });
                        return true;
                    }

                    if (e.Time >= outlierAvgStart)
                    {
                        statsCount++;
                        if (e.IsOutlier)
                            outliersCount++;
                    }
                }

                var avg = outliersCount / (decimal) statsCount;
                if (avg > repeatedOutliersParams.MaxAvg)
                {
                    _alertService.AlertRiskOfficer(assetPairId,
                        $"{orderbook.ExchangeName} is a repeated outlier exchange for {assetPairId}.\r\n" +
                        $"It had {avg * 100:f4}% (i.e. {outliersCount} / {statsCount}) of max " +
                        $"{repeatedOutliersParams.MaxAvg * 100:f4}% " +
                        $"outlier orderbooks during last {repeatedOutliersParams.MaxAvgAge.TotalSeconds:f0} secs.");
                    Trace.Write(TraceLevelGroupEnum.ErrorTrace, assetPairId, "Repeated outlier (avg)",
                        new
                        {
                            Event = "Repeated outlier",
                            Reason = "Avg",
                            orderbook.ExchangeName,
                            outliersCount,
                            statsCount,
                            avg,
                            repeatedOutliersParams.MaxAvg,
                            repeatedOutliersParams.MaxAvgAge
                        });
                    return true;
                }
            }

            return false;
        }

        private static ImmutableSortedSet<Event> AddEventAndCleanOld(ImmutableSortedSet<Event> events, Event ev,
            DateTime minEventTime)
        {
            if (events[0].Time < minEventTime)
                return events.SkipWhile(e => e.Time < minEventTime).Concat(new[] {ev})
                    .ToImmutableSortedSet(Event.ComparerByTime);
            else
                return events.Add(ev);
        }

        private class Event
        {
            public DateTime Time { get; }
            public bool IsOutdated { get; }
            public bool IsOutlier { get; }

            public Event(DateTime time, bool isOutdated, bool isOutlier)
            {
                Time = time;
                IsOutdated = isOutdated;
                IsOutlier = isOutlier;
            }

            private sealed class TimeComparer : IComparer<Event>
            {
                public int Compare(Event x, Event y)
                {
                    return Comparer<DateTime?>.Default.Compare(x?.Time, y?.Time);
                }
            }

            public static IComparer<Event> ComparerByTime { get; } = new TimeComparer();
        }
    }
}