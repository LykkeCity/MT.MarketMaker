using System.Threading.Tasks;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Services.ExtPrices.Implementation
{
    internal class AlertService : IAlertService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IReloadingManager<MarginTradingMarketMakerSettings> _settings;
        private readonly IMtMmRisksSlackNotificationsSender _slack;

        public AlertService(IRabbitMqService rabbitMqService,
            IReloadingManager<MarginTradingMarketMakerSettings> settings, IMtMmRisksSlackNotificationsSender slack)
        {
            _rabbitMqService = rabbitMqService;
            _settings = settings;
            _slack = slack;
        }

        public void AlertPrimaryExchangeSwitched(PrimaryExchangeSwitchedMessage message)
        {
            message.MarketMakerId = GetMarketMakerId();
            _rabbitMqService.GetProducer<PrimaryExchangeSwitchedMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.PrimaryExchangeSwitched), false)
                .ProduceAsync(message);
        }

        public void StopOrAllowNewTrades(string assetPairId, string reason, bool stop)
        {
            _rabbitMqService.GetProducer<StopOrAllowNewTradesMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.StopNewTrades), false)
                .ProduceAsync(new StopOrAllowNewTradesMessage
                {
                    AssetPairId = assetPairId,
                    MarketMakerId = GetMarketMakerId(),
                    Reason = reason,
                    Stop = stop
                });

            AlertRiskOfficer(assetPairId + ' ' + nameof(StopOrAllowNewTrades), $"{(stop ? "Stop" : "Allow")}NewTrades for {assetPairId} because: {reason}");
        }

        public Task AlertRiskOfficer(string assetPairId, string message)
        {
            Trace.Write(nameof(AlertRiskOfficer) + ' ' + assetPairId, $"{nameof(AlertRiskOfficer)}: {message}");
            return _slack.SendAsync(null, "MarketMaker", message);
        }

        public void AlertStarted()
        {
            AlertRiskOfficer(null, "Market maker started");
            var startedMessage = new StartedMessage {MarketMakerId = GetMarketMakerId()};
            _rabbitMqService.GetProducer<StartedMessage>(
                    _settings.Nested(s => s.RabbitMq.Publishers.Started), true)
                .ProduceAsync(startedMessage);
        }

        public Task AlertStopping()
        {
            return Task.WhenAll(
                AlertRiskOfficer(null, "Market maker stopping"),
                _rabbitMqService.GetProducer<StoppingMessage>(
                        _settings.Nested(s => s.RabbitMq.Publishers.Stopping), true)
                    .ProduceAsync(new StoppingMessage {MarketMakerId = GetMarketMakerId()}));
        }

        private string GetMarketMakerId()
        {
            return _settings.CurrentValue.MarketMakerId;
        }
    }
}