using System;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using FluentAssertions;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Enums;
using NUnit.Framework;

namespace Tests.Integrational.Controllers
{
    public class CrossRateCalcInfosControllerTests
    {
        private readonly MmIntegrationalTestSuit _testSuit = new MmIntegrationalTestSuit();
        
        [Test]
        public void IfCrossRateCalcInfoNotExists_ShouldGetNull()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<CrossRateCalcInfosController>().AsSelf());
            var container = env.CreateContainer();
            var crossRateCalcInfosController = container.Resolve<CrossRateCalcInfosController>();

            //act
            var info = crossRateCalcInfosController.Get("non-existent pair");
            
            //assert
            info.Should().BeNull();
        }
        
        [Test]
        public void OnPairCreation_ShouldAutogenerateCrossRatesCalcInfo()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<AssetPairsController>().AsSelf());
            env.Setup(b => b.RegisterType<CrossRateCalcInfosController>().AsSelf());
            var container = env.CreateContainer();
            var assetPairsController = container.Resolve<AssetPairsController>();
            var crossRateCalcInfosController = container.Resolve<CrossRateCalcInfosController>();
            env.AssetPairs.Add(new AssetPair
            {
                Id = "pair",
                BaseAssetId = "base",
                QuotingAssetId = "quoting",
                Source = "src1",
                Source2 = "src2"
            });
            env.AssetPairs.Add(new AssetPair
            {
                Id = "src1",
                BaseAssetId = "base-src1",
                QuotingAssetId = "pair"
            });
            env.AssetPairs.Add(new AssetPair
            {
                Id = "src2",
                BaseAssetId = "pair",
                QuotingAssetId = "quoting-src2"
            });

            //act
            assetPairsController.Add("pair", AssetPairQuotesSourceTypeDomainEnum.Disabled);
            var info = crossRateCalcInfosController.Get("pair");
            
            //assert
            info.Should().BeEquivalentTo(new CrossRateCalcInfoModel
            {
                ResultingPairId = "pair",
                Source1 = new CrossRateSourceAssetPairModel
                {
                    Id = "src1",
                    IsTransitoryAssetQuoting = true,
                },
                Source2 = new CrossRateSourceAssetPairModel
                {
                    Id = "src2",
                    IsTransitoryAssetQuoting = false,
                },
            });
        }
    }
}