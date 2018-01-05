using System.Collections.Immutable;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MarginTrading.MarketMaker.Services.ExtPrices;
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
        public void IfSourceIsCrossRates_ShouldReturnCorrectResults()
        {
            //arrange
            var ethUsdCalcInfo = new CrossRateCalcInfo("ETHUSD",
                new CrossRateSourceAssetPair("ETHBTC", true),
                new CrossRateSourceAssetPair("BTCUSD", false));
            var btcEurCalcInfo = new CrossRateCalcInfo("BTCEUR",
                new CrossRateSourceAssetPair("BTCUSD", true),
                new CrossRateSourceAssetPair("EURUSD", true));
            var btcChfCalcInfo = new CrossRateCalcInfo("BTCCHF",
                new CrossRateSourceAssetPair("BTCUSD", true),
                new CrossRateSourceAssetPair("USDCHF", false));

            var ethBtcOrderbook = MakeOrderbook("ETHBTC");
            var eurUsdOrderbook = MakeOrderbook("EURUSD");
            var usdChfOrderbook = MakeOrderbook("USDCHF");
            var btcUsdOrderbook = MakeOrderbook("BTCUSD");

            _testSuit
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("ETHBTC") == new[] {ethUsdCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("EURUSD") == new[] {btcEurCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("USDCHF") == new[] {btcChfCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s =>
                    s.GetDependentAssetPairs("BTCUSD") == new[] {ethUsdCalcInfo, btcEurCalcInfo, btcChfCalcInfo})
                .Setup<IBestPricesService>(s => s.Calc(ethBtcOrderbook) == new BestPrices(0.04m, 0.05m))
                .Setup<IBestPricesService>(s => s.Calc(eurUsdOrderbook) == new BestPrices(1.2m, 1.3m))
                .Setup<IBestPricesService>(s => s.Calc(usdChfOrderbook) == new BestPrices(0.98m, 0.99m))
                .Setup<IBestPricesService>(s => s.Calc(btcUsdOrderbook) == new BestPrices(6500, 6600))
                .Setup<IAssetPairSourceTypeService>(s => s.Get(It.IsNotNull<string>()) == AssetPairQuotesSourceTypeDomainEnum.CrossRates);

            //act
            var ethBtcResult = _testSuit.Sut.CalcDependentOrderbooks(ethBtcOrderbook);
            var eurUsdResult = _testSuit.Sut.CalcDependentOrderbooks(eurUsdOrderbook);
            var usdChfResult = _testSuit.Sut.CalcDependentOrderbooks(usdChfOrderbook);
            var btcUsdResult = _testSuit.Sut.CalcDependentOrderbooks(btcUsdOrderbook);

            //assert
            ethBtcResult.Should().BeEmpty();
            eurUsdResult.Should().BeEmpty();
            usdChfResult.Should().BeEmpty();
            //BTCEUR = BTCUSD * (ask EURUSD)^-1
            //BTCCHF = BTCUSD * USDCHF
            //ETHUSD = ETHBTC * BTCUSD
            btcUsdResult.ShouldAllBeEquivalentTo(
                new[]
                {
                    new Orderbook("BTCEUR",
                        ImmutableArray.Create(new OrderbookPosition(6500 * (1 / 1.3m), 1)),
                        ImmutableArray.Create(new OrderbookPosition(6600 * (1 / 1.2m), 1))),
                    new Orderbook("BTCCHF",
                        ImmutableArray.Create(new OrderbookPosition(6500 * 0.98m, 1)),
                        ImmutableArray.Create(new OrderbookPosition(6600 * 0.99m, 1))),
                    new Orderbook("ETHUSD",
                        ImmutableArray.Create(new OrderbookPosition(0.04m * 6500, 1)),
                        ImmutableArray.Create(new OrderbookPosition(0.05m * 6600, 1))),
                });
        }

        [Test]
        public void IfSourceIsNotCrossRates_ShouldNotCalculateCrossRates()
        {
            //arrange
            var ethUsdCalcInfo = new CrossRateCalcInfo("ETHUSD",
                new CrossRateSourceAssetPair("ETHBTC", true),
                new CrossRateSourceAssetPair("BTCUSD", false));
            var btcEurCalcInfo = new CrossRateCalcInfo("BTCEUR",
                new CrossRateSourceAssetPair("BTCUSD", true),
                new CrossRateSourceAssetPair("EURUSD", true));
            var btcChfCalcInfo = new CrossRateCalcInfo("BTCCHF",
                new CrossRateSourceAssetPair("BTCUSD", true),
                new CrossRateSourceAssetPair("USDCHF", false));

            var ethBtcOrderbook = MakeOrderbook("ETHBTC");
            var eurUsdOrderbook = MakeOrderbook("EURUSD");
            var usdChfOrderbook = MakeOrderbook("USDCHF");
            var btcUsdOrderbook = MakeOrderbook("BTCUSD");

            _testSuit
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("ETHBTC") == new[] {ethUsdCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("EURUSD") == new[] {btcEurCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s => s.GetDependentAssetPairs("USDCHF") == new[] {btcChfCalcInfo})
                .Setup<ICrossRateCalcInfosService>(s =>
                    s.GetDependentAssetPairs("BTCUSD") == new[] {ethUsdCalcInfo, btcEurCalcInfo, btcChfCalcInfo})
                .Setup<IAssetPairSourceTypeService>(s => s.Get(It.IsNotNull<string>()) == AssetPairQuotesSourceTypeDomainEnum.External);

            //act
            var ethBtcResult = _testSuit.Sut.CalcDependentOrderbooks(ethBtcOrderbook);
            var eurUsdResult = _testSuit.Sut.CalcDependentOrderbooks(eurUsdOrderbook);
            var usdChfResult = _testSuit.Sut.CalcDependentOrderbooks(usdChfOrderbook);
            var btcUsdResult = _testSuit.Sut.CalcDependentOrderbooks(btcUsdOrderbook);

            //assert
            ethBtcResult.Should().BeEmpty();
            eurUsdResult.Should().BeEmpty();
            usdChfResult.Should().BeEmpty();
            btcUsdResult.Should().BeEmpty();
        }

        private static Orderbook MakeOrderbook(string assetPairId)
        {
            return new Orderbook(assetPairId,
                ImmutableArray<OrderbookPosition>.Empty, ImmutableArray<OrderbookPosition>.Empty);
        }
    }
}