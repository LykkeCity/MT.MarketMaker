using System;
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
                .Setup<ICrossRatesSettingsRepository>(m => m.Setup(s => s.GetAllAsync()).ReturnsAsync(crossRatesSettings));

            //act
            var result = suit.Sut.Get();

            //assert
            result.ShouldAllBeEquivalentTo(crossRatesSettings);
        }

        [Test]
        public void Write_Always_ShouldWriteToProviderAndToCache()
        {
            //arrange
            var emptyCrossRatesSettings = GetEmptySettings();
            var newCrossRatesSettings = GetFilledSettings();

            IEnumerable<CrossRatesSettings> writtenSettings = null;
            var suit = _testSuit
                .Setup<ICrossRatesSettingsRepository>(m => m.Setup(s => s.GetAllAsync()).ReturnsAsync(emptyCrossRatesSettings))
                .Setup<ICrossRatesSettingsRepository>(m => m
                    .Setup(s => s.InsertOrReplaceAsync(It.IsNotNull<IEnumerable<CrossRatesSettings>>()))
                    .Callback<IEnumerable<CrossRatesSettings>>(s => writtenSettings = s)
                    .Returns(Task.CompletedTask));

            //act
            suit.Sut.Set(newCrossRatesSettings);
            var readed = suit.Sut.Get();

            //assert
            writtenSettings.ShouldAllBeEquivalentTo(newCrossRatesSettings);
            readed.ShouldAllBeEquivalentTo(newCrossRatesSettings);
        }

        private static IReadOnlyList<CrossRatesSettings> GetEmptySettings()
        {
            return Array.Empty<CrossRatesSettings>();
        }

        private static IReadOnlyList<CrossRatesSettings> GetFilledSettings()
        {
            return new[]
            {
                new CrossRatesSettings("base1", ImmutableArray.Create("other1", "other2")),
                new CrossRatesSettings("base2", ImmutableArray.Create("other3", "other4")),
            };
        }
    }
}
