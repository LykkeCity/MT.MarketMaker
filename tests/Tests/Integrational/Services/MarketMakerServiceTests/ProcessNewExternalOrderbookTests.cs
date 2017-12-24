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
            var decimals = Generate.Decimals();
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
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitfinex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));

            //assert
            env.PrintLogs();
            var decimals = Generate.Decimals();
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
            IContainer container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "bitfinex"));
            var bitmexDecimals = Generate.FromLambda<decimal>((o, i) => (i + 1) * 1.1m);
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(bitmexDecimals, "bitmex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Poloniex"));
            await marketMakerService.ProcessNewExternalOrderbookAsync(GetMessage(Generate.Decimals(), "Kraken"));

            //assert
            env.PrintLogs();
            var decimals = Generate.Decimals();
            bitmexDecimals.Reset();
            env.VerifyCommandsSent(("BTCUSD", decimals.Take(4), decimals.Take(4)), ("BTCUSD", bitmexDecimals.Take(4), bitmexDecimals.Take(4)));
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