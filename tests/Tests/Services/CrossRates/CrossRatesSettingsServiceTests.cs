using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.CrossRates.Implementation;
using Moq;
using NUnit.Framework;

namespace Tests.Services.CrossRates
{
    public class CrossRatesSettingsServiceTests
    {
        private static readonly TestSuit<CrossRatesSettingsService> _testSuit = TestSuit.Create<CrossRatesSettingsService>();

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        [Test]
        public void Read_Always_ShouldReturnValidResult()
        {
            //arrange
            var crossRatesSettings = GetFilledSettings();
            var suit = _testSuit
                .Setup<ICrossRatesSettingsRepository>(m => MockExtensions.ReturnsAsync(m.Setup(s => s.GetAllAsync()), new[] { crossRatesSettings }));

            //act
            var result = suit.Sut.Get();

            //assert
            AssetSettingsEqual(result, crossRatesSettings);
        }

        [Test]
        public void Write_Always_ShouldWriteToProviderAndToCache()
        {
            //arrange
            var emptyCrossRatesSettings = GetEmptySettings();
            var newCrossRatesSettings = GetFilledSettings();

            CrossRatesSettings writtenSettings = null;
            var suit = _testSuit
                .Setup<ICrossRatesSettingsRepository>(m => MockExtensions.ReturnsAsync(m.Setup(s => s.GetAllAsync()), new []{ emptyCrossRatesSettings }))
                .Setup<ICrossRatesSettingsRepository>(m => m
                    .Setup(s => s.InsertOrReplaceAsync(It.IsNotNull<CrossRatesSettings>()))
                    .Callback<CrossRatesSettings>(s => writtenSettings = s)
                    .Returns(Task.CompletedTask));

            //act
            suit.Sut.Set(newCrossRatesSettings);
            var readed = suit.Sut.Get();

            //assert
            AssetSettingsEqual(writtenSettings, newCrossRatesSettings);
            AssetSettingsEqual(readed, newCrossRatesSettings);
        }

        private static void AssetSettingsEqual(CrossRatesSettings current, CrossRatesSettings expected)
        {
            current.Should().NotBeNull();
            current.BaseAssetsIds.Should().BeEquivalentTo(expected.BaseAssetsIds);
            current.OtherAssetsIds.Should().BeEquivalentTo(expected.OtherAssetsIds);
        }

        private static CrossRatesSettings GetEmptySettings()
        {
            return new CrossRatesSettings(ImmutableArray<string>.Empty, ImmutableArray<string>.Empty);
        }

        private static CrossRatesSettings GetFilledSettings()
        {
            return new CrossRatesSettings(ImmutableArray.Create("base1", "base2"), ImmutableArray.Create("other1", "other2"));
        }
    }
}
