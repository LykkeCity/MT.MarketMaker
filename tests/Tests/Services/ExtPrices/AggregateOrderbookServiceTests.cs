using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.ExtPrices;
using MarginTrading.MarketMaker.Services.ExtPrices.Implementation;
using NUnit.Framework;
using Tests.Integrational;

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
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(3);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook);

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(3);
            result.Asks.Sum(a => a.Volume).Should().Be(3);

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
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(4);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(3);
            result.Asks.Sum(a => a.Volume).Should().Be(3);

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
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(45);
            result.Asks.Sum(a => a.Volume).Should().Be(45);

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
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(45);
            result.Asks.Sum(a => a.Volume).Should().Be(45);

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
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(45);
            result.Asks.Sum(a => a.Volume).Should().Be(45);

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

        [Test]
        public void IfOneAggregatedLevelIsConsumedByAnother_ShouldSkipIt()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(2, 
                        ImmutableSortedSet.Create(3m, 10m, 9m, 25m, 40m), 0));
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.FromLambda<double>((old, i) => 0)});

            var originalOrderbook = GetTestOrderbook(10);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(45);
            result.Asks.Sum(a => a.Volume).Should().Be(45);

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

        [Test]
        public void IfRandomEnabled_ShouldAggregateRandomly()
        {
            //arrange
            _testSuit
                .Setup<IExtPricesSettingsService>(s =>
                    s.IsStepEnabled(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, "pair") &&
                    s.GetAggregateOrderbookSettings("pair") ==
                    new AggregateOrderbookSettings(0,
                        ImmutableSortedSet.Create(3m, 10, 25, 40), 0.6m));
            _testSuit.Setup<ISystem>(s =>
                s.GetRandom() == new TestRandom {Doubles = Generate.Values(0.8, 0.2, 1, 0.5)});
            
            var originalOrderbook = GetTestOrderbook(11);

            //act
            var result = _testSuit.Sut.Aggregate(originalOrderbook).Trace();

            //assert
            result.Bids.Sum(a => a.Volume).Should().Be(55);
            result.Asks.Sum(a => a.Volume).Should().Be(55);

            // note we keep best prices, but cut excessive volume with worst prices
            result.Should().BeEquivalentTo(new Orderbook("pair", 
                ImmutableArray.Create(
                    new OrderbookPosition(10000, 3),
                    new OrderbookPosition(7000, 12),
                    new OrderbookPosition(6000, 6),
                    new OrderbookPosition(2000, 34)),
                ImmutableArray.Create(
                    new OrderbookPosition(12001, 3), 
                    new OrderbookPosition(15001, 12), 
                    new OrderbookPosition(16001, 6),
                    new OrderbookPosition(20001, 34))));
        }
    }
}