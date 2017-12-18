using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using FluentAssertions;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Services.Common;
using Newtonsoft.Json;
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
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitmex"));

            //assert
            container.PrintLogs();
            decimals.Reset();
            env.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4)));
            env.VerifyTradesStopped("BTCUSD", true);
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex");
        }

        [Test]
        public async Task SimpleConfig_ShouldProcessMultipleValidMessages()
        {
            //arrange
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var decimals = Generate.Decimals();
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitfinex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Kraken"));

            //assert
            container.PrintLogs();
            decimals.Reset();
            env.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4)));
            env.VerifyTradesStopped("BTCUSD", true);
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex", "bitfinex"); // note sort by name among equal prio exchanges
        }
        
        [Test]
        public async Task BitmexHashableButArrivesSecond_ShouldStopTradesAndThenSwitchToBitmex()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            var container = env.CreateContainer();
            var extPriceExchangesController = container.Resolve<ExtPriceExchangesController>();
            var bitmexSettings = extPriceExchangesController.Get("BTCUSD", "bitmex").RequiredNotNull("bitmexSettings");
            bitmexSettings.Hedging.DefaultPreference = 1;
            extPriceExchangesController.Update(bitmexSettings);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var decimals = Generate.Decimals();
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitfinex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(decimals, "Kraken"));

            //assert
            container.PrintLogs();
            decimals.Reset();
            env.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4))); // todo: why?
            env.VerifyTradesStopped("BTCUSD", false, true);
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitfinex", "bitmex");
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