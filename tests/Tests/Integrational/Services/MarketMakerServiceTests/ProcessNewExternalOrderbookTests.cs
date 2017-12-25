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
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitmex"));

            //assert
            env.PrintLogs();
            env.VerifyCommandsSent(("BTCUSD", Generate.Decimals()));
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
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitfinex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));

            //assert
            env.PrintLogs();
            env.VerifyCommandsSent(("BTCUSD", Generate.Decimals()));
            env.VerifyTradesStopped("BTCUSD", true);
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex", "bitfinex"); // note sort by name among equal prio exchanges
        }
        
        [Test]
        public async Task BitmexHashableButArrivesSecond_ShouldStopTradesAndThenSwitchToBitmex()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            IContainer container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitfinex"));
            var bitmexDecimals = Generate.Decimals(multiplier: 1.049m); // note default outlier threshold 5%
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(bitmexDecimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));

            //assert
            env.PrintLogs();
            env.VerifyCommandsSent(("BTCUSD", Generate.Decimals()), ("BTCUSD", bitmexDecimals));
            env.VerifyTradesStopped("BTCUSD", true, false);
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitfinex", "bitmex");
        }
        
        [Test]
        public async Task BitmexPrimary_BecomesOutlier_ShouldSkipUpdates()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            IContainer container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var bitmexDecimals = Generate.Decimals(multiplier: 1.05m); // note default outlier threshold 5%
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(bitmexDecimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(multiplier: 1.06m), "bitmex"));

            //assert
            env.PrintLogs();
            env.VerifyCommandsSent(("BTCUSD", bitmexDecimals));
            env.VerifyTradesStopped("BTCUSD"); // should send no StopOrAllowNewTradesMessages
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex");
        }
        
        [Test]
        public async Task BitmexPrimaryOutlier_BecomesOk_ShouldSendUpdate()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            IContainer container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var bitmexDecimals = Generate.Decimals(multiplier: 1.05m); // note default outlier threshold 5%
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(bitmexDecimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));
            var bitmexDecimals2 = Generate.Decimals(multiplier: 1.049m);
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(bitmexDecimals2, "bitmex"));

            //assert
            env.PrintLogs();
            env.VerifyCommandsSent(("BTCUSD", bitmexDecimals), ("BTCUSD", bitmexDecimals2));
            env.VerifyTradesStopped("BTCUSD"); // should send no StopOrAllowNewTradesMessages
            env.VerifyPrimaryExchangeSwitched("BTCUSD", "bitmex");
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