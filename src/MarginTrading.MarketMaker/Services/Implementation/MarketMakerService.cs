using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Filters;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    internal class MarketMakerService : IMarketMakerService, IDisposable
    {
        private const int OrdersVolume = 1000000;

        private readonly IAssetPairsSettingsService _assetPairsSettingsService;
        private readonly Lazy<IMessageProducer<OrderCommandsBatchMessage>> _messageProducer;
        private readonly ISystem _system;
        private readonly IReloadingManager<MarginTradingMarketMakerSettings> _settings;
        private readonly ISpotOrderCommandsGeneratorService _spotOrderCommandsGeneratorService;
        private readonly ILog _log;
        private readonly IGenerateOrderbookService _generateOrderbookService;
        private readonly ICrossRatesService _crossRatesService;

        public MarketMakerService(IAssetPairsSettingsService assetPairsSettingsService,
            IRabbitMqService rabbitMqService,
            ISystem system,
            IReloadingManager<MarginTradingMarketMakerSettings> settings,
            ISpotOrderCommandsGeneratorService spotOrderCommandsGeneratorService,
            ILog log,
            IGenerateOrderbookService generateOrderbookService,
            ICrossRatesService crossRatesService)
        {
            _assetPairsSettingsService = assetPairsSettingsService;
            _system = system;
            _settings = settings;
            _spotOrderCommandsGeneratorService = spotOrderCommandsGeneratorService;
            _log = log;
            _generateOrderbookService = generateOrderbookService;
            _crossRatesService = crossRatesService;
            _messageProducer = new Lazy<IMessageProducer<OrderCommandsBatchMessage>>(() =>
                CreateRabbitMqMessageProducer(settings, rabbitMqService));
        }

        public Task ProcessNewExternalOrderbookAsync(ExternalExchangeOrderbookMessage orderbook)
        {
            var quotesSource = _assetPairsSettingsService.GetAssetPairQuotesSource(orderbook.AssetPairId);
            if (quotesSource != AssetPairQuotesSourceTypeEnum.External
                || (orderbook.Bids?.Count ?? 0) == 0 || (orderbook.Asks?.Count ?? 0) == 0)
            {
                return Task.CompletedTask;
            }

            var externalOrderbook = new ExternalOrderbook(orderbook.AssetPairId, orderbook.Source, _system.UtcNow,
                orderbook.Bids.Select(b => new OrderbookPosition(b.Price, b.Volume)).ToImmutableArray(),
                orderbook.Asks.Select(b => new OrderbookPosition(b.Price, b.Volume)).ToImmutableArray());
            var resultingOrderbook = _generateOrderbookService.OnNewOrderbook(externalOrderbook);
            if (resultingOrderbook == null)
            {
                return Task.CompletedTask;
            }

            var orderbooksToSend = _crossRatesService.CalcDependentOrderbooks(resultingOrderbook)
                .Add(resultingOrderbook);

            return SendOrderCommandsAsync(orderbooksToSend);
        }

        public Task ProcessNewSpotOrderBookDataAsync(SpotOrderbookMessage orderbook)
        {
            var quotesSource = _assetPairsSettingsService.GetAssetPairQuotesSource(orderbook.AssetPair);
            if (quotesSource != AssetPairQuotesSourceTypeEnum.Spot || (orderbook.Prices?.Count ?? 0) == 0)
            {
                return Task.CompletedTask;
            }

            var commands = _spotOrderCommandsGeneratorService.GenerateOrderCommands(orderbook.AssetPair, orderbook.IsBuy,
                orderbook.Prices[0].Price, OrdersVolume);

            return SendOrderCommandsAsync(orderbook.AssetPair, commands);
        }

        public async Task ProcessAssetPairSettingsAsync(AssetPairSettingsInputModel model)
        {
            AssetPairQuotesSourceTypeEnum? quotesSourceType;
            if (model.SetSourceType != null)
            {
                quotesSourceType = model.SetSourceType.Value;
                if (quotesSourceType == AssetPairQuotesSourceTypeEnum.Manual)
                {
                    TestFunctionalityFilter.ValidateTestsEnabled();
                }

                await _assetPairsSettingsService.SetAssetPairQuotesSourceAsync(model.AssetPairId,
                    model.SetSourceType.Value);
            }
            else
            {
                quotesSourceType = _assetPairsSettingsService.GetAssetPairQuotesSource(model.AssetPairId);
            }

            if (quotesSourceType == AssetPairQuotesSourceTypeEnum.Manual && model.PriceForBuyOrder != null &&
                model.PriceForSellOrder != null)
            {
                var commands = new[]
                {
                    new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder},
                    new OrderCommand
                    {
                        CommandType = OrderCommandTypeEnum.SetOrder,
                        Direction = OrderDirectionEnum.Sell,
                        Price = model.PriceForSellOrder.Value,
                        Volume = OrdersVolume
                    },
                    new OrderCommand
                    {
                        CommandType = OrderCommandTypeEnum.SetOrder,
                        Direction = OrderDirectionEnum.Buy,
                        Price = model.PriceForBuyOrder.Value,
                        Volume = OrdersVolume
                    },
                };
                await SendOrderCommandsAsync(model.AssetPairId, commands);
            }
        }

        private static IMessageProducer<OrderCommandsBatchMessage> CreateRabbitMqMessageProducer(
            IReloadingManager<MarginTradingMarketMakerSettings> settings, IRabbitMqService rabbitMqService)
        {
            return rabbitMqService.GetProducer<OrderCommandsBatchMessage>(
                settings.Nested(s => s.RabbitMq.Publishers.OrderCommands), false);
        }

        private Task SendOrderCommandsAsync(IEnumerable<Orderbook> orderbooksToSend)
        {
            // todo: send batches of batches (because of cross-rates)
            return Task.WhenAll(orderbooksToSend
                .Select(o =>
                {
                    var commands = new List<OrderCommand>
                    {
                        new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder}
                    };

                    foreach (var bid in o.Bids)
                    {
                        commands.Add(new OrderCommand
                        {
                            CommandType = OrderCommandTypeEnum.SetOrder,
                            Direction = OrderDirectionEnum.Buy,
                            Price = bid.Price,
                            Volume = bid.Volume
                        });
                    }

                    foreach (var ask in o.Asks)
                    {
                        commands.Add(new OrderCommand
                        {
                            CommandType = OrderCommandTypeEnum.SetOrder,
                            Direction = OrderDirectionEnum.Sell,
                            Price = ask.Price,
                            Volume = ask.Volume
                        });
                    }

                    return SendOrderCommandsAsync(o.AssetPairId, commands);
                }));
        }

        private Task SendOrderCommandsAsync(string assetPairId, IReadOnlyList<OrderCommand> commands)
        {
            if (commands.Count == 0)
            {
                return Task.CompletedTask;
            }

            return _messageProducer.Value.ProduceAsync(new OrderCommandsBatchMessage
            {
                AssetPairId = assetPairId,
                Timestamp = _system.UtcNow,
                Commands = commands,
                MarketMakerId = _settings.CurrentValue.MarketMakerId,
            });
        }

        public void Dispose()
        {
            var tasks = _assetPairsSettingsService.GetAllPairsSourcesAsync().GetAwaiter().GetResult()
                .Select(p => SendOrderCommandsAsync(p.AssetPairId, new []{ new OrderCommand { CommandType = OrderCommandTypeEnum.DeleteOrder } }))
                .ToArray();
            Task.WaitAll(tasks);
        }
    }
}