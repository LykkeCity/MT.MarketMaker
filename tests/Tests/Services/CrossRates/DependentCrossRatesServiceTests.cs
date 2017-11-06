using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.CrossRates;
using MarginTrading.MarketMaker.Services.CrossRates.Implementation;
using Microsoft.Rest;
using NUnit.Framework;

namespace Tests.Services.CrossRates
{
    public class DependentCrossRatesServiceTests
    {
        private static readonly TestSuit<DependentCrossRatesService> _testSuit = TestSuit.Create<DependentCrossRatesService>();

        private static readonly IList<AssetPairResponseModel> AssetPairs = new[]
        {
            new AssetPairResponseModel
            {
                Accuracy = 3,
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                InvertedAccuracy = 8,
                IsDisabled = false,
                Name = "BTC/EUR",
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
            new AssetPairResponseModel
            {
                Accuracy = 3,
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                InvertedAccuracy = 8,
                IsDisabled = false,
                Name = "BTC/EUR",
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
            new AssetPairResponseModel
            {
                Accuracy = 3,
                BaseAssetId = "BTC",
                Id = "BTCEUR",
                InvertedAccuracy = 8,
                IsDisabled = false,
                Name = "BTC/EUR",
                QuotingAssetId = "EUR",
                Source = "BTCUSD",
                Source2 = "EURUSD"
            },
        };

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        [Test]
        public void Always_ShouldDoSmth()
        {
            //arrange
            var crossRatesSettings = new CrossRatesSettings(ImmutableArray.Create("BTC, USD"), ImmutableArray.Create("EUR", "LKK"));
            _testSuit
                .Setup<ICrossRatesSettingsService>(p => p.Get() == crossRatesSettings)
                .Setup<IAssetsservice>(p => p.GetAssetPairsWithHttpMessagesAsync(null, CancellationToken.None) == AssetPairs.ToResponse());

            //act
            var result = _testSuit.Sut.GetDependentAssetPairs("srcPair").ToList();

            //assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}
