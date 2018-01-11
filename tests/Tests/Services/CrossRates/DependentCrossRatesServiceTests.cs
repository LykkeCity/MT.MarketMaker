using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using NUnit.Framework;

namespace Tests.Services.CrossRates
{
    public class DependentCrossRatesServiceTests
    {
        private static readonly TestSuit<DependentCrossRatesService> _testSuit =
            TestSuit.Create<DependentCrossRatesService>();

        private static readonly DateTime _now = DateTime.UtcNow;

        private static readonly IReadOnlyDictionary<string, AssetPairInfo> AssetPairs = new[]
        {
            new AssetPairInfo
            {
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
            new AssetPairInfo
            {
                BaseAssetId = "BTC",
                Id = "BTCAUD",
                QuotingAssetId = "AUD",
                Source = "BTCUSD",
                Source2 = "AUDUSD"
            },
            new AssetPairInfo
            {
                BaseAssetId = "ETH",
                Id = "ETHUSD",
                QuotingAssetId = "USD",
                Source = "ETHBTC",
                Source2 = "BTCUSD"
            },
            new AssetPairInfo
            {
                BaseAssetId = "ETH",
                Id = "ETHBTC",
                QuotingAssetId = "BTC",
            },
            new AssetPairInfo
            {
                BaseAssetId = "AUD",
                Id = "AUDUSD",
                QuotingAssetId = "USD",
            },
            new AssetPairInfo
            {
                BaseAssetId = "EUR",
                Id = "EURUSD",
                QuotingAssetId = "USD",
            },
            new AssetPairInfo
            {
                BaseAssetId = "BTC",
                Id = "BTCUSD",
                QuotingAssetId = "USD",
            },
        }.ToDictionary(p => p.Id);

        private static readonly CrossRatesSettings[] CrossRatesSettings =
        {
            new CrossRatesSettings("BTC", ImmutableArray.Create("EUR", "AUD", "ETH")),
            new CrossRatesSettings("USD", ImmutableArray.Create("EUR", "AUD", "ETH")),
        };

        [SetUp]
        public void SetUp()
        {
            _testSuit.Reset();
            _testSuit.Setup<ISystem>(s => s.UtcNow == _now);
        }

        [TestCase("BTCUSD", ExpectedResult = new[] {"BTCEUR", "BTCAUD", "ETHUSD"})]
        [TestCase("EURUSD", ExpectedResult = new[] {"BTCEUR"})]
        [TestCase("AUDUSD", ExpectedResult = new[] {"BTCAUD"})]
        [TestCase("ETHBTC", ExpectedResult = new[] {"ETHUSD"})]
        public string[] Always_ShouldCorrectlyChooseResultingPair(string assetPairId)
        {
            //arrange
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == CrossRatesSettings)
                .Setup<IAssetPairsInfoService>(p => p.Get() == AssetPairs);

            //act
            return _testSuit.Sut.GetDependentAssetPairs(assetPairId).Select(i => i.ResultingPairId).ToArray();
        }

        [Test]
        public void Btcusd_ShouldCorrectlyFillResult()
        {
            //arrange
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == CrossRatesSettings)
                .Setup<IAssetPairsInfoService>(p => p.Get() == AssetPairs);

            //act
            var result = _testSuit.Sut.GetDependentAssetPairs("BTCUSD").ToArray();

            //assert
            result.Should().BeEquivalentTo(new List<CrossRateCalcInfo>
            {
                new CrossRateCalcInfo("BTCEUR", new CrossRateSourceAssetPair("BTCUSD", true),
                    new CrossRateSourceAssetPair("EURUSD", true)),
                new CrossRateCalcInfo("BTCAUD", new CrossRateSourceAssetPair("BTCUSD", true),
                    new CrossRateSourceAssetPair("AUDUSD", true)),
                new CrossRateCalcInfo("ETHUSD", new CrossRateSourceAssetPair("ETHBTC", true),
                    new CrossRateSourceAssetPair("BTCUSD", false)),
            });
        }
    }
}