using System.Collections.Immutable;
using FluentAssertions;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using Moq;
using NUnit.Framework;

namespace Tests.Services.CrossRates
{
    public class CrossRatesServiceTests
    {
        private static readonly TestSuit<CrossRatesService> _testSuit = TestSuit.Create<CrossRatesService>();

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        [Test]
        public void Always_ShouldDoSmth()
        {
            //arrange

            var ethUsdCalcInfo = new CrossRateCalcInfo("ETHUSD", new CrossRateSourceAssetPair("ETHBTC", true),
                new CrossRateSourceAssetPair("BTCUSD", false));
            var btcEurCalcInfo = new CrossRateCalcInfo("BTCEUR", new CrossRateSourceAssetPair("BTCUSD", true),
                new CrossRateSourceAssetPair("EURUSD", true));

            var ethBtcOrderbook = MakeOrderbook("ETHBTC");
            var eurUsdOrderbook = MakeOrderbook("EURUSD");
            var btcUsdOrderbook = MakeOrderbook("BTCUSD");

            _testSuit
                .Setup<IDependentCrossRatesService>(s => s.GetDependentAssetPairs("ETHBTC") == new[] {ethUsdCalcInfo})
                .Setup<IDependentCrossRatesService>(s => s.GetDependentAssetPairs("EURUSD") == new[] {btcEurCalcInfo})
                .Setup<IDependentCrossRatesService>(s => s.GetDependentAssetPairs("BTCUSD") == new[] {ethUsdCalcInfo, btcEurCalcInfo})
                .Setup<IBestPricesService>(s => s.Calc(ethBtcOrderbook) == new BestPrices(0.04m, 0.05m))
                .Setup<IBestPricesService>(s => s.Calc(eurUsdOrderbook) == new BestPrices(1.2m, 1.3m))
                .Setup<IBestPricesService>(s => s.Calc(btcUsdOrderbook) == new BestPrices(6500, 6600));

            //act
            var ethBtcResult = _testSuit.Sut.CalcDependentOrderbooks(ethBtcOrderbook);
            var eurUsdResult = _testSuit.Sut.CalcDependentOrderbooks(eurUsdOrderbook);
            var btcUsdResult = _testSuit.Sut.CalcDependentOrderbooks(btcUsdOrderbook);

            //assert
            ethBtcResult.Should().BeEmpty();
            eurUsdResult.Should().BeEmpty();
            btcUsdResult.ShouldAllBeEquivalentTo(
                new[]
                {
                    new Orderbook("ETHUSD",
                        ImmutableArray.Create(new OrderbookPosition(259.999999999999999999999922M, 1)),
                        ImmutableArray.Create(new OrderbookPosition(329.9999999999999999999998944M, 1))),
                    new Orderbook("BTCEUR",
                        ImmutableArray.Create(new OrderbookPosition(5416.6666666666666666666666667M, 1)),
                        ImmutableArray.Create(new OrderbookPosition(5076.9230769230769230769230769M, 1))),
                });
        }

        private static Orderbook MakeOrderbook(string assetPairId)
        {
            return new Orderbook(assetPairId,
                ImmutableArray<OrderbookPosition>.Empty, ImmutableArray<OrderbookPosition>.Empty);
        }
    }
}