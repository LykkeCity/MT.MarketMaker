using System;
using System.Collections.ObjectModel;
using System.Linq;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Settings;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class AlertSeverityLevelService : IAlertSeverityLevelService
    {
        private readonly IReloadingManager<ReadOnlyCollection<(EventTypeEnum Event, 
            (string SlackChannelType, TraceLevelGroupEnum TraceLevel) Level)>> _levels;

        private static readonly (string, TraceLevelGroupEnum AlertRiskOfficerInfo) _defaultLevel =
            ("mt-critical", TraceLevelGroupEnum.AlertRiskOfficerInfo);

        public AlertSeverityLevelService(IReloadingManager<RiskInformingSettings> settings)
        {
            _levels = settings.Nested(s =>
            {
                return s.Data.Where(d => d.System == "MarketMaker")
                    .Select(d => (ConvertEventTypeCode(d.EventTypeCode), ConvertLevel(d.Level)))
                    .ToList().AsReadOnly();
            });
        }

        private static EventTypeEnum ConvertEventTypeCode(string eventTypeCode)
        {
            switch (eventTypeCode)
            {
                case "MM01": return EventTypeEnum.StopNewTrades;
                case "MM02": return EventTypeEnum.PrimaryExchangeChanged;
                case "MM03": return EventTypeEnum.OutlierDetected;
                case "MM04": return EventTypeEnum.ExchangeDisabled;
                case "MM05": return EventTypeEnum.StatusInfo;
                case "MM06": return EventTypeEnum.InvalidSettingsFound;
                default:
                    throw new ArgumentOutOfRangeException(nameof(RiskInformingParams.EventTypeCode), eventTypeCode, null);
            }
        }

        private static (string SlackChannelType, TraceLevelGroupEnum TraceLevel) ConvertLevel(string alertSeverityLevel)
        {
            switch (alertSeverityLevel)
            {
                case "None":
                    return (null, TraceLevelGroupEnum.AlertRiskOfficerInfo);
                case "Information":
                    return ("mt-information", TraceLevelGroupEnum.AlertRiskOfficerCrit);
                case "Warning":
                    return ("mt-warning", TraceLevelGroupEnum.AlertRiskOfficerWarn);
                default:
                    return _defaultLevel;
            }
        }

        public (string SlackChannelType, TraceLevelGroupEnum TraceLevel) GetLevel(EventTypeEnum eventType)
        {
            return _levels.CurrentValue.Where(l => l.Event == eventType).Select(l => l.Level)
                .FallbackIfEmpty(_defaultLevel).Single();
        }
    }
}