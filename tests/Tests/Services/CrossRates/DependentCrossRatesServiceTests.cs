using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
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

        private static readonly IList<AssetPairResponseModel> AssetPairs = new[]
        {
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCAUD",
                QuotingAssetId = "AUD",
                Source = "BTCUSD",
                Source2 = "AUDUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "ETH",
                Id = "ETHUSD",
                QuotingAssetId = "USD",
                Source = "ETHBTC",
                Source2 = "BTCUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "ETH",
                Id = "ETHBTC",
                QuotingAssetId = "BTC",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "AUD",
                Id = "AUDUSD",
                QuotingAssetId = "USD",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "EUR",
                Id = "EURUSD",
                QuotingAssetId = "USD",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCUSD",
                QuotingAssetId = "USD",
            },
        };

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
                .Setup<IAssetsService>(p =>
                    p.GetAssetPairsWithHttpMessagesAsync(null, CancellationToken.None) == AssetPairs.ToResponse());

            //act
            return _testSuit.Sut.GetDependentAssetPairs(assetPairId).Select(i => i.ResultingPairId).ToArray();
        }

        [Test]
        public void Btcusd_ShouldCorrectlyFillResult()
        {
            //arrange
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == CrossRatesSettings)
                .Setup<IAssetsService>(p =>
                    p.GetAssetPairsWithHttpMessagesAsync(null, CancellationToken.None) == AssetPairs.ToResponse());

            //act
            var result = _testSuit.Sut.GetDependentAssetPairs("BTCUSD").ToArray();

            //assert
            result.ShouldAllBeEquivalentTo(new List<CrossRateCalcInfo>
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