using System;
using System.Threading.Tasks;
using Lykke.SettingsReader;
using Lykke.SlackNotifications;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Settings;
using Microsoft.ApplicationInsights.DataContracts;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    internal class AlertService : IAlertService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IReloadingManager<MarginTradingMarketMakerSettings> _settings;
        private readonly ISlackNotificationsSender _slack;
        private readonly IAlertSeverityLevelService _alertSeverityLevelService;

        public AlertService(IRabbitMqService rabbitMqService,
            IReloadingManager<MarginTradingMarketMakerSettings> settings, ISlackNotificationsSender slack,
            IAlertSeverityLevelService alertSeverityLevelService)
        {
            _rabbitMqService = rabbitMqService;
            _settings = settings;
            _slack = slack;
            _alertSeverityLevelService = alertSeverityLevelService;
        }

        public void AlertPrimaryExchangeSwitched(PrimaryExchangeSwitchedMessage message)
        {
            message.MarketMakerId = GetMarketMakerId();
            _rabbitMqService.GetProducer<PrimaryExchangeSwitchedMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.PrimaryExchangeSwitched), false, false)
                .ProduceAsync(message);
        }

        public void StopOrAllowNewTrades(string assetPairId, string reason, bool stop)
        {
            _rabbitMqService.GetProducer<StopOrAllowNewTradesMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.StopNewTrades), false, false)
                .ProduceAsync(new StopOrAllowNewTradesMessage
                {
                    AssetPairId = assetPairId,
                    MarketMakerId = GetMarketMakerId(),
                    Reason = reason,
                    Stop = stop
                });

            AlertRiskOfficer(assetPairId + ' ' + nameof(StopOrAllowNewTrades), $"{(stop ? "Stop" : "Allow")}NewTrades for {assetPairId} because: {reason}", EventTypeEnum.StopNewTrades);
        }

        public void AlertRiskOfficer(string assetPairId, string message, EventTypeEnum eventType)
        {
            var (slackChannelType, traceLevel) = _alertSeverityLevelService.GetLevel(eventType);
            Trace.Write(traceLevel, assetPairId, $"{nameof(AlertRiskOfficer)}: {message}", new {});
            _slack.SendAsync(slackChannelType, "MT MarketMaker", message);
        }

        public void AlertStarted()
        {
            AlertRiskOfficer(null, "Market maker started", EventTypeEnum.StatusInfo);
            var startedMessage = new StartedMessage {MarketMakerId = GetMarketMakerId()};
            _rabbitMqService.GetProducer<StartedMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.Started), true, false)
                .ProduceAsync(startedMessage);
        }

        public Task AlertStopping()
        {
            AlertRiskOfficer(null, "Market maker stopping", EventTypeEnum.StatusInfo);
            return _rabbitMqService.GetProducer<StoppingMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.Stopping), true, false)
                .ProduceAsync(new StoppingMessage {MarketMakerId = GetMarketMakerId()});
        }

        private string GetMarketMakerId()
        {
            return _settings.CurrentValue.MarketMakerId;
        }
    }
}