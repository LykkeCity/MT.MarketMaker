using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Services.Common;
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
            var testContainerBuilder = _testSuit.Build();
            var marketMakerService = testContainerBuilder.CreateContainer().Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitmex"));

            //assert
            decimals.Reset();
            testContainerBuilder.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4)));
            testContainerBuilder.VerifyTradesStopped("BTCUSD", true);
            testContainerBuilder.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex");
        }

        [Test]
        public async Task SimpleConfig_ShouldProcessMultipleValidMessages()
        {
            //arrange
            var decimals = Generate.Decimals();
            var testContainerBuilder = _testSuit.Build();
            var marketMakerService = testContainerBuilder.CreateContainer().Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitfinex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Kraken"));

            //assert
            decimals.Reset();
            testContainerBuilder.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4)));
            testContainerBuilder.VerifyTradesStopped("BTCUSD", true);
            testContainerBuilder.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex", "bitfinex"); // note sort by name among equal prio exchanges
        }

        private static ExternalExchangeOrderbookMessage GetMessage(Generator<decimal> decimals, string exchangeName)
        {
            return new ExternalExchangeOrderbookMessage
            {
                Bids = new List<VolumePrice>
                {
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()},
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()}
                },
                Asks = new List<VolumePrice>
                {
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()},
                    new VolumePrice {Price = decimals.Next(), Volume = decimals.Next()}
                },
                AssetPairId = "BTCUSD",
                Source = exchangeName,
                Timestamp = DateTime.UtcNow.AddSeconds(-1),
            };
        }
    }
}