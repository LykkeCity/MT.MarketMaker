using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.ExtPrices;
using MarginTrading.MarketMaker.Services.ExtPrices.Implementation;
using NUnit.Framework;

namespace Tests.Services.ExtPrices
{
    public class AggregateOrderbookServiceTests
    {
        private static readonly TestSuit<AggregateOrderbookService> _testSuit =
            TestSuit.Create<AggregateOrderbookService>();

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        private static Orderbook GetTestOrderbook(int size)
        {
            var volumes = Generate.Decimals();
            var bids = Enumerable.Range(1, size).Reverse().Select(p => new OrderbookPosition(p * 1000, volumes.Next()))
                .ToImmutableArray();
            var bestAsk = bids.First().Price + 1;
            volumes.Reset();
            var asks = Enumerable.Range(0, size).Select(p => new OrderbookPosition(bestAsk + p * 1000, volumes.Next()))
                .ToImmutableArray();
            volumes.Reset();
            // "Bids":[{"Price":2000.0,"Volume":1},{"Price":1000.0,"Volume":2}],
            // "Asks":[{"Price":2001.0,"Volume":1},{"Price":3001.0,"Volume":2}]
            return new Orderbook("pair", bids, asks).Trace();
        }

        [Test]
        public void IfStepDisabled_ShouldReturnSame()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") == false);

            var originalOrderbook = GetTestOrderbook(2);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook);

            //assert
            result.Should().Be(originalOrderbook);
        }

        [TestCase(3)]
        [TestCase(4)]
        public void IfSourceIsSmallerOrEqualThenAsIsLevelsCount_ShouldReturnSame(int asIsLevelsCount)
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(asIsLevelsCount, ImmutableSortedSet<decimal>.Empty, 0));

            var originalOrderbook = GetTestOrderbook(3);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook);

            //assert
            result.Should().BeEquivalentTo(originalOrderbook);
        }

        [Test]
        public void IfNoAggregatedLevelsConfigured_ShouldReturnSameWithTrimmedCount()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, ImmutableSortedSet<decimal>.Empty, 0));

            var originalOrderbook = GetTestOrderbook(3);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook);

            //assert

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", result.Bids.Take(2).ToImmutableArray(),
                result.Asks.Take(2).ToImmutableArray()));
        }

        [Test]
        public void IfAggregatedLevelIsConsumedByAsIsLevels_ShouldNotGenerateIt()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, 
                        ImmutableSortedSet.Create(3m), 0));

            var originalOrderbook = GetTestOrderbook(4);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", 
                ImmutableArray.Create(new OrderbookPosition(4000, 1), new OrderbookPosition(3000, 2)),
                ImmutableArray.Create(new OrderbookPosition(4001, 1), new OrderbookPosition(5001, 2))));
        }

        [Test]
        public void IfAggregatedLevelIsNotConsumedByAsIsLevels_ShouldAggregate()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, 
                        ImmutableSortedSet.Create(10m, 25m, 40m), 0));

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", 
                ImmutableArray.Create(new OrderbookPosition(10000, 1), new OrderbookPosition(9000, 2), // as is
                    new OrderbookPosition(7000, 7),
                    new OrderbookPosition(4000, 18),
                    new OrderbookPosition(2000, 17)), // note last level trimmed
                ImmutableArray.Create(new OrderbookPosition(10001, 1), new OrderbookPosition(11001, 2), // as is
                    new OrderbookPosition(13001, 7), 
                    new OrderbookPosition(16001, 18),
                    new OrderbookPosition(18001, 17))));
        }

        [Test]
        public void IfSingleOrderFillsWholeLevel_ShouldPutAsIs()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, 
                        ImmutableSortedSet.Create(3m, 10m, 11m, 25m, 40m), 0));

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", 
                ImmutableArray.Create(new OrderbookPosition(10000, 1), new OrderbookPosition(9000, 2), // as is
                    new OrderbookPosition(7000, 7), // note aggr level 3 gets consumed by as-is levels
                    new OrderbookPosition(6000, 5),
                    new OrderbookPosition(4000, 13),
                    new OrderbookPosition(2000, 17)), // note last level trimmed
                ImmutableArray.Create(new OrderbookPosition(10001, 1), new OrderbookPosition(11001, 2), // as is
                    new OrderbookPosition(13001, 7), 
                    new OrderbookPosition(14001, 5),
                    new OrderbookPosition(16001, 13),
                    new OrderbookPosition(18001, 17))));
        }

        [Test]
        public void IfHasAggregatedLevelsConfigured_ShouldAggregate()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, 
                        ImmutableSortedSet.Create(3m, 10m, 25m, 40m), 0));

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", 
                ImmutableArray.Create(new OrderbookPosition(10000, 1), new OrderbookPosition(9000, 2), // as is
                    new OrderbookPosition(7000, 7), // note aggr level 3 gets consumed by as-is levels
                    new OrderbookPosition(4000, 18),
                    new OrderbookPosition(2000, 17)), // note last level trimmed
                ImmutableArray.Create(new OrderbookPosition(10001, 1), new OrderbookPosition(11001, 2), // as is
                    new OrderbookPosition(13001, 7), 
                    new OrderbookPosition(16001, 18),
                    new OrderbookPosition(18001, 17))));
        }
    }
}