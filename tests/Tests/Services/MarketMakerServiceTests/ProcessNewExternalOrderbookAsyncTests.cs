using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Common;
using FluentAssertions;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.Common.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.ExtPrices;
using MarginTrading.MarketMaker.Settings;
using Moq;
using NUnit.Framework;

namespace Tests.Services.MarketMakerServiceTests
{
    public class ProcessNewExternalOrderbookAsyncTests
    {
        private static readonly TestSuit<MarketMakerService> _testSuit = TestSuit.Create<MarketMakerService>();
        private static readonly DateTime _now = DateTime.UtcNow;

        private List<OrderCommandsBatchMessage> _sentMessages;

        [SetUp]
        public void SetUp()
        {
            _sentMessages = new List<OrderCommandsBatchMessage>();
            _testSuit.Reset();
            _testSuit.Setup<IRabbitMqService>(s =>
                    s.GetProducer<OrderCommandsBatchMessage>(
                        It.IsNotNull<IReloadingManager<RabbitConnectionSettings>>(), false, false)
                    == _testSuit.GetMockObj<IMessageProducer<OrderCommandsBatchMessage>>())
                .Setup<IMessageProducer<OrderCommandsBatchMessage>>(mock => mock.Setup(
                        p => p.ProduceAsync(It.IsNotNull<OrderCommandsBatchMessage>()))
                    .Returns(Task.CompletedTask)
                    .Callback<OrderCommandsBatchMessage>(m => _sentMessages.Add(m)));
        }

        [TestCase(AssetPairQuotesSourceTypeDomainEnum.Manual)]
        [TestCase(AssetPairQuotesSourceTypeDomainEnum.Spot)]
        public async Task IfQuotesSourceNotExternal_ShouldSkip(AssetPairQuotesSourceTypeDomainEnum configuredSource)
        {
            //arrange
            _testSuit
                .Setup<IAssetPairSourceTypeService>(s => s.Get("pair") == configuredSource);

            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Asks = new List<VolumePrice> {new VolumePrice()},
                Bids = new List<VolumePrice> {new VolumePrice()},
                Source = "source",
            };

            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfBidsNull_ShouldSkip()
        {
            //arrange
            _testSuit.Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External);

            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Asks = new List<VolumePrice> {new VolumePrice()},
                Bids = null,
                Source = "source",
            };

            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfBidsAreEmpty_ShouldSkip()
        {
            //arrange
            _testSuit
                .Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External);

            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Asks = new List<VolumePrice> {new VolumePrice()},
                Bids = new List<VolumePrice>(),
                Source = "source",
            };

            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfAsksNull_ShouldSkip()
        {
            //arrange
            _testSuit
                .Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External);

            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Asks = null,
                Bids = new List<VolumePrice> {new VolumePrice()},
                Source = "source",
            };

            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfAsksAreEmpty_ShouldSkip()
        {
            //arrange
            _testSuit
                .Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External);
            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Asks = new List<VolumePrice>(),
                Bids = new List<VolumePrice> {new VolumePrice()},
                Source = "source",
            };

            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfResultingOrderbookIsNull_ShouldSkip()
        {
            //arrange
            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Bids = new List<VolumePrice>
                {
                    new VolumePrice {Price = 1, Volume = 2},
                    new VolumePrice {Price = 3, Volume = 4}
                },
                Asks = new List<VolumePrice>
                {
                    new VolumePrice {Price = 5, Volume = 6},
                    new VolumePrice {Price = 7, Volume = 8}
                },
                Source = "source",
            };

            var externalOrderbook = new ExternalOrderbook("pair", "source", _now,
                ImmutableArray.Create(new OrderbookPosition(1, 2), new OrderbookPosition(3, 4)),
                ImmutableArray.Create(new OrderbookPosition(5, 6), new OrderbookPosition(7, 8)));

            _testSuit
                .Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External)
                .Setup<ISystem>(s => s.UtcNow == _now)
                .Setup<IGenerateOrderbookService>(s => s.OnNewOrderbook(externalOrderbook.Equivalent()) == null);


            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfResultingOrderbookNotNull_ShouldSendItWithDependentOnes()
        {
            //arrange
            var incomingMessage = new ExternalExchangeOrderbookMessage
            {
                AssetPairId = "pair",
                Bids = new List<VolumePrice>
                {
                    new VolumePrice {Price = 1, Volume = 2},
                    new VolumePrice {Price = 3, Volume = 4}
                },
                Asks = new List<VolumePrice>
                {
                    new VolumePrice {Price = 5, Volume = 6},
                    new VolumePrice {Price = 7, Volume = 8}
                },
                Source = "source",
            };

            var externalOrderbook = new ExternalOrderbook("pair", "source", _now,
                ImmutableArray.Create(new OrderbookPosition(1, 2), new OrderbookPosition(3, 4)),
                ImmutableArray.Create(new OrderbookPosition(5, 6), new OrderbookPosition(7, 8)));

            var dependentOrderbooks = ImmutableList.Create(
                new Orderbook("dependent pair 1",
                    ImmutableArray.Create(new OrderbookPosition(11, 12), new OrderbookPosition(13, 14)),
                    ImmutableArray.Create(new OrderbookPosition(15, 16), new OrderbookPosition(17, 18))),
                new Orderbook("dependent pair 2",
                    ImmutableArray.Create(new OrderbookPosition(21, 22), new OrderbookPosition(23, 24)),
                    ImmutableArray.Create(new OrderbookPosition(25, 26), new OrderbookPosition(27, 28))));
            var resultingOrderbook = new Orderbook("resulting pair",
                ImmutableArray.Create(new OrderbookPosition(31, 32), new OrderbookPosition(33, 34)),
                ImmutableArray.Create(new OrderbookPosition(35, 36), new OrderbookPosition(37, 38)));

            _testSuit
                .Setup<IAssetPairSourceTypeService>(s =>
                    s.Get("pair") == AssetPairQuotesSourceTypeDomainEnum.External)
                .Setup<ISystem>(s => s.UtcNow == _now)
                .Setup<IGenerateOrderbookService>(s =>
                    s.OnNewOrderbook(externalOrderbook.Equivalent()) == resultingOrderbook)
                .Setup<ICrossRatesService>(s => s.CalcDependentOrderbooks(resultingOrderbook) == dependentOrderbooks)
                .Setup<IReloadingManager<MarginTradingMarketMakerSettings>>(s =>
                    s.CurrentValue == new MarginTradingMarketMakerSettings {MarketMakerId = "mm id"});


            //act
            await _testSuit.Sut.ProcessNewExternalOrderbookAsync(incomingMessage);

            //assert
            var expectation = new List<OrderCommandsBatchMessage>
            {
                MakeOrderCommandsBatchMessage("dependent pair 1", 10),
                MakeOrderCommandsBatchMessage("dependent pair 2", 20),
                MakeOrderCommandsBatchMessage("resulting pair", 30),
            };
            AssertionExtensions.ShouldAllBeEquivalentTo<OrderCommandsBatchMessage>(_sentMessages, expectation);
        }

        public static OrderCommandsBatchMessage MakeOrderCommandsBatchMessage(string pairId, int m)
        {
            return new OrderCommandsBatchMessage
            {
                AssetPairId = pairId,
                Timestamp = _now,
                Commands = new List<OrderCommand>
                {
                    new OrderCommand{CommandType = OrderCommandTypeEnum.DeleteOrder},
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 1, Volume = m + 2, Direction = OrderDirectionEnum.Buy},
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 3, Volume = m + 4, Direction = OrderDirectionEnum.Buy},
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 5, Volume = m + 6, Direction = OrderDirectionEnum.Sell},
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 7, Volume = m + 8, Direction = OrderDirectionEnum.Sell},
                },
                MarketMakerId = "mm id",
            };
        }
    }
}