using System;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.ExtPrices;
using Moq;
using NUnit.Framework;

namespace Tests.Integrational.Controllers
{
    public class ExtPriceSettingsControllerTests
    {
        private readonly MmIntegrationalTestSuit _testSuit = new MmIntegrationalTestSuit();
        
        [Test]
        public void Always_ShouldCorrectlyUpdatePairSettings()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceSettingsController>().AsSelf());
            env.Setup(b => b.RegisterType<AssetPairsController>().AsSelf());
            var container = env.CreateContainer();
            var assetPairsController = container.Resolve<AssetPairsController>();
            var extPriceSettingsController = container.Resolve<ExtPriceSettingsController>();
            
            var model = new AssetPairExtPriceSettingsModel
            {
                AssetPairId = "pair",
                Markups = new AssetPairMarkupsParamsModel {Bid = 1, Ask = 2},
                MinOrderbooksSendingPeriod = TimeSpan.FromMinutes(3),
                OutlierThreshold = 4,
                PresetDefaultExchange = "ex",
                RepeatedOutliers = new RepeatedOutliersParamsModel
                {
                    MaxAvg = 5,
                    MaxAvgAge = TimeSpan.FromMinutes(6),
                    MaxSequenceLength = 7,
                    MaxSequenceAge = TimeSpan.FromMinutes(8),
                },
                Steps = ImmutableSortedDictionary.Create<OrderbookGeneratorStepEnum, bool>()
                    .Add(OrderbookGeneratorStepEnum.FindOutliers, false),
            };

            //act
            assetPairsController.Add("pair", AssetPairQuotesSourceTypeDomainEnum.Disabled);
            env.Sleep(new TimeSpan(1));
            extPriceSettingsController.Update(model);
            var extractedModel = extPriceSettingsController.Get("pair");
            
            //assert
            model.Steps = GetDefaultSteps().SetItems(model.Steps);
            extractedModel.Should().BeEquivalentTo(model);
        }
        
        private static ImmutableSortedDictionary<OrderbookGeneratorStepEnum, bool> GetDefaultSteps()
        {
            return Enum.GetValues(typeof(OrderbookGeneratorStepEnum)).Cast<OrderbookGeneratorStepEnum>()
                .ToImmutableSortedDictionary(e => e, e => true);
        }
    }
}