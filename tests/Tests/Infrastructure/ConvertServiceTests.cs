using System;
using System.Collections.Immutable;
using AutoMapper;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;
using NUnit.Framework;

namespace Tests.Infrastructure
{
    public class ConvertServiceTests
    {
        private static readonly TestSuit<ConvertService> _testSuit = TestSuit.Create<ConvertService>();

        [SetUp]
        public void SetUp()
        {
            _testSuit.Reset();
        }

        [Test]
        public void Always_ShouldConvertAssetPairExtPriceSettings()
        {
            //arrange
            var model = new AssetPairExtPriceSettingsModel
            {
                AssetPairId = "pair",
                Markups = new AssetPairMarkupsParamsModel {Bid = 1, Ask = 2},
                MinOrderbooksSendingPeriod = TimeSpan.FromMinutes(3),
                OutlierThreshold = 4,
                PresetDefaultExchange = "ex",
                RepeatedOutliers = new RepeatedOutliersParamsModel
                {TL
                    MaxAvg = 5,
                    MaxAvgAge = TimeSpan.FromMinutes(6),
                    MaxSequenceLength = 7,
                    MaxSequenceAge = TimeSpan.FromMinutes(8),
                },
                Steps = ImmutableDictionary.Create<OrderbookGeneratorStepEnum, bool>()
                    .Add(OrderbookGeneratorStepEnum.FindOutliers, false),
            };

            //act
            var settings = _testSuit.Sut.Convert<AssetPairExtPriceSettingsModel, AssetPairExtPriceSettings>(model, o =>
                o.ConfigureMap(MemberList.Destination).ForCtorParam("exchanges",
                        e => e.ResolveUsing(m => ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty))
                    .ForMember(e => e.Exchanges, e => e.Ignore()));

            var result = _testSuit.Sut.Convert<AssetPairExtPriceSettings, AssetPairExtPriceSettingsModel>(settings,
                o => o.ConfigureMap(MemberList.Source).ForSourceMember(s => s.Exchanges, e => e.Ignore()));

            //assert
            settings.Exchanges.Should().BeEmpty();
            result.AssetPairId.Should().BeNull();
            result.ShouldBeEquivalentTo(model, o => o.Excluding(m => m.AssetPairId));
        }
    }
}