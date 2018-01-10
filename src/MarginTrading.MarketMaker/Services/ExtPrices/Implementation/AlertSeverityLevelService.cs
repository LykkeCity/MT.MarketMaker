using System;
using System.Collections.ObjectModel;
using System.Linq;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    public class AlertSeverityLevelService : IAlertSeverityLevelService
    {
        private readonly IReloadingManager<ReadOnlyCollection<(EventTypeEnum Event, 
            (string SlackChannelType, TraceLevelGroupEnum TraceLevel) Level)>> _levels;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(RiskInformingParams.EventTypeCode), eventTypeCode, null);
            }
        }

        private static (string SlackChannelType, TraceLevelGroupEnum TraceLevel) ConvertLevel(AlertSeverityLevel alertSeverityLevel)
        {
            switch (alertSeverityLevel)
            {
                case AlertSeverityLevel.None:
                    return (null, TraceLevelGroupEnum.AlertRiskOfficerInfo);
                case AlertSeverityLevel.Information:
                    return ("mt-information", TraceLevelGroupEnum.AlertRiskOfficerInfo);
                case AlertSeverityLevel.Warning:
                    return ("mt-warning", TraceLevelGroupEnum.AlertRiskOfficerWarn);
                case AlertSeverityLevel.Critical:
                    return ("mt-critical", TraceLevelGroupEnum.AlertRiskOfficerCrit);
                default:
                    throw new ArgumentOutOfRangeException(nameof(RiskInformingParams.Level), alertSeverityLevel, null);
            }
        }

        public (string SlackChannelType, TraceLevelGroupEnum TraceLevel) GetLevel(EventTypeEnum eventType)
        {
            return _levels.CurrentValue.Single(l => l.Event == eventType).Level;
        }
    }
}