using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.Common.Implementation;
using NUnit.Framework;

namespace Tests.Integrational.Services.MarketMakerServiceTests
{
    public class ProcessNewExternalOrderbookTests
    {
        private readonly MmIntegrationalTestSuit _testSuit = new MmIntegrationalTestSuit();

        [Test]
        public async Task SimpleConfig_ShouldProcessSingleMessage()
        {
            //arrange
            var decimals = Generate.Decimals();
            var message = new ExternalExchangeOrderbookMessage
            {
                Bids = new List<VolumePrice> { new VolumePrice{Price = decimals.Next(), Volume = decimals.Next()}, new VolumePrice{Price = decimals.Next(), Volume = decimals.Next()}},
                Asks = new List<VolumePrice> { new VolumePrice{Price = decimals.Next(), Volume = decimals.Next()}, new VolumePrice{Price = decimals.Next(), Volume = decimals.Next()}},
                AssetPairId = "BTCUSD",
                Source = "bitmex",
                Timestamp = DateTime.UtcNow.AddSeconds(-1),
            };

            //act
            var testContainerBuilder = _testSuit.Build();
            var container = testContainerBuilder.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();
            await marketMakerService.ProcessNewExternalOrderbookAsync(message);

            //assert
            decimals.Reset();
            testContainerBuilder.StubRabbitMqService.GetSentMessages<OrderCommandsBatchMessage>()
                .ShouldAllBeEquivalentTo(testContainerBuilder.GetExpectedCommands(("BTCUSD", decimals.Take(4), decimals.Take(4))));
        }
    }
}