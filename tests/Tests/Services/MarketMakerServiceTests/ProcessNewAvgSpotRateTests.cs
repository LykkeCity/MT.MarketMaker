using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Common;
using FluentAssertions;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.Common.Implementation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Settings;
using Moq;
using NUnit.Framework;

namespace Tests.Services.MarketMakerServiceTests
{
    public class ProcessNewAvgSpotRateTests
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
                        It.IsNotNull<IReloadingManager<RabbitConnectionSettings>>(), false)
                    == _testSuit.GetMockObj<IMessageProducer<OrderCommandsBatchMessage>>())
                .Setup<IMessageProducer<OrderCommandsBatchMessage>>(mock => mock.Setup(
                        p => p.ProduceAsync(It.IsNotNull<OrderCommandsBatchMessage>()))
                    .Returns(Task.CompletedTask)
                    .Callback<OrderCommandsBatchMessage>(m => _sentMessages.Add(m)));
        }

        [TestCase(AssetPairQuotesSourceTypeEnum.Manual)]
        [TestCase(AssetPairQuotesSourceTypeEnum.External)]
        [TestCase(AssetPairQuotesSourceTypeEnum.Spot)]
        [TestCase(AssetPairQuotesSourceTypeEnum.Disabled)]
        [TestCase(AssetPairQuotesSourceTypeEnum.CrossRates)]
        public async Task IfPairSourceNotNull_ShouldSkip(AssetPairQuotesSourceTypeEnum sourceType)
        {
            //arrange
            _testSuit
                .Setup<IAssetPairSourceTypeService>(s => s.Get("pair") == sourceType)
                .Setup<ISystem>(s => s.UtcNow == _now)
                .Setup<IReloadingManager<MarginTradingMarketMakerSettings>>(s =>
                    s.CurrentValue == new MarginTradingMarketMakerSettings {MarketMakerId = "mm id"});

            //act
            await _testSuit.Sut.ProcessNewAvgSpotRate("pair", 1, 2);

            //assert
            _sentMessages.Should().BeEmpty();
        }

        [Test]
        public async Task IfPairSourceIsSpotAgvPrices_ShouldSendValidMessage()
        {
            //arrange
            var resultingOrderbook = new Orderbook("pair",
                ImmutableArray.Create(new OrderbookPosition(1, 1000000)),
                ImmutableArray.Create(new OrderbookPosition(2, 1000000)));

            var crossOrderbooks = ImmutableList.Create(
                new Orderbook("cross pair 1",
                    ImmutableArray.Create(new OrderbookPosition(11, 1000010)),
                    ImmutableArray.Create(new OrderbookPosition(12, 1000010))),
                new Orderbook("cross pair 2",
                    ImmutableArray.Create(new OrderbookPosition(21, 1000020)),
                    ImmutableArray.Create(new OrderbookPosition(22, 1000020))));

            _testSuit
                .Setup<IAssetPairSourceTypeService>(s => s.Get("pair") == AssetPairQuotesSourceTypeEnum.SpotAgvPrices)
                .Setup<ISystem>(s => s.UtcNow == _now)
                .Setup<IReloadingManager<MarginTradingMarketMakerSettings>>(s =>
                    s.CurrentValue == new MarginTradingMarketMakerSettings {MarketMakerId = "mm id"})
                .Setup<ICrossRatesService>(s => s.CalcDependentOrderbooks(resultingOrderbook.Equivalent()) == crossOrderbooks);

            //act
            await _testSuit.Sut.ProcessNewAvgSpotRate("pair", 1, 2);

            //assert
            var expectation = new List<OrderCommandsBatchMessage>
            {
                MakeOrderCommandsBatchMessage("pair", 0),
                MakeOrderCommandsBatchMessage("cross pair 1", 10),
                MakeOrderCommandsBatchMessage("cross pair 2", 20),
            };
            _sentMessages.ShouldAllBeEquivalentTo(expectation);
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
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 1, Volume = 1000000 + m, Direction = OrderDirectionEnum.Buy},
                    new OrderCommand{CommandType = OrderCommandTypeEnum.SetOrder, Price = m + 2, Volume = 1000000 + m, Direction = OrderDirectionEnum.Sell},
                },
                MarketMakerId = "mm id",
            };
        }
    }
}