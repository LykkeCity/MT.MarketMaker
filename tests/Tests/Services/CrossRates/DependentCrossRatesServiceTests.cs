using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Models;
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

        private static readonly IList<AssetPairResponseModel> AssetPairs = new[]
        {
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                IsDisabled = false,
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCAUD",
                IsDisabled = false,
                QuotingAssetId = "AUD",
                Source = "BTCUSD",
                Source2 = "AUDUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "ETH",
                Id = "ETHUSD",
                IsDisabled = false,
                QuotingAssetId = "USD",
                Source = "ETHBTC",
                Source2 = "BTCUSD"
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "ETH",
                Id = "ETHBTC",
                IsDisabled = false,
                QuotingAssetId = "BTC",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "AUD",
                Id = "AUDUSD",
                IsDisabled = false,
                QuotingAssetId = "USD",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "EUR",
                Id = "EURUSD",
                IsDisabled = false,
                QuotingAssetId = "USD",
            },
            new AssetPairResponseModel
            {
                BaseAssetId = "BTC",
                Id = "BTCUSD",
                IsDisabled = false,
                QuotingAssetId = "USD",
            },
        };

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        [TestCase("BTCUSD", ExpectedResult = new[] {"BTCEUR", "BTCAUD", "ETHUSD"})]
        [TestCase("EURUSD", ExpectedResult = new[] {"BTCEUR"})]
        [TestCase("AUDUSD", ExpectedResult = new[] {"BTCAUD"})]
        [TestCase("ETHBTC", ExpectedResult = new[] {"ETHUSD"})]
        public string[] Always_ShouldCorrectlyChooseResultingPair(string assetPairId)
        {
            //arrange
            var crossRatesSettings = new CrossRatesSettings(ImmutableArray.Create("BTC", "USD"),
                ImmutableArray.Create("EUR", "AUD", "ETH"));
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == crossRatesSettings)
                .Setup<IAssetsservice>(p =>
                    p.GetAssetPairsWithHttpMessagesAsync(null, CancellationToken.None) == AssetPairs.ToResponse());

            //act
            return _testSuit.Sut.GetDependentAssetPairs(assetPairId).Select(i => i.ResultingPairId).ToArray();
        }

        [Test]
        public void Btcusd_ShouldCorrectlyFillResult()
        {
            //arrange
            var crossRatesSettings = new CrossRatesSettings(ImmutableArray.Create("BTC", "USD"),
                ImmutableArray.Create("EUR", "AUD", "ETH"));
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == crossRatesSettings)
                .Setup<IAssetsservice>(p =>
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