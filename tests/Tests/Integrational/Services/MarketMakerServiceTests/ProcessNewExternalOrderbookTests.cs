using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Controllers;
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
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", Generate.Decimals()));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSent(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                env.GetExpectedTradesControls("BTCUSD", true),
                env.GetExpectedCommandsBatch("BTCUSD", Generate.Decimals())
            );
        }

        [Test]
        public async Task SimpleConfig_ShouldProcessMultipleValidMessages()
        {
            //arrange
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("bitfinex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("Poloniex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", Generate.Decimals()));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSent(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                env.GetExpectedTradesControls("BTCUSD", true),
                env.GetExpectedCommandsBatch("BTCUSD", Generate.Decimals()),
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitfinex"));
        }

        [Test]
        public async Task BitmexHashableButArrivesSecond_ShouldStopTradesAndThenSwitchToBitmex()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            var container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("bitfinex", Generate.Decimals()));
            var bitmexDecimals = Generate.Decimals(1.049m); // note outlier threshold 5%
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("Poloniex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", Generate.Decimals()));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSent(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitfinex"),
                env.GetExpectedTradesControls("BTCUSD", true),
                env.GetExpectedCommandsBatch("BTCUSD", Generate.Decimals()),
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                env.GetExpectedTradesControls("BTCUSD", false),
                env.GetExpectedCommandsBatch("BTCUSD", bitmexDecimals));
        }

        [Test]
        public async Task BitmexPrimary_BecomesOutlier_ShouldSkipUpdates()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            var container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var bitmexDecimals = Generate.Decimals(1.05m); // note outlier threshold 5%
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("Poloniex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", Generate.Decimals()));
            var bitmexDecimals2 = Generate.Decimals(1.06m);
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals2));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSent(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                env.GetExpectedCommandsBatch("BTCUSD", bitmexDecimals));
            env.StubRabbitMqService.GetSentMessages<StopOrAllowNewTradesMessage>().Should().BeEmpty();
        }

        [Test]
        public async Task BitmexPrimaryOutlier_BecomesOk_ShouldSendUpdate()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            var container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var bitmexDecimals = Generate.Decimals(1.05m); // note outlier threshold 5%
            var firstExpectedBatch = env.GetExpectedCommandsBatch("BTCUSD", bitmexDecimals);
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("Poloniex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", Generate.Decimals()));
            var bitmexDecimals2 = Generate.Decimals(1.049m);
            env.SleepSecs(5);
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals2));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSentWithTime(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                firstExpectedBatch,
                env.GetExpectedCommandsBatch("BTCUSD", bitmexDecimals2));
            env.StubRabbitMqService.GetSentMessages<StopOrAllowNewTradesMessage>().Should().BeEmpty();
        }

        [Test]
        public async Task BitmexPrimary_BecomesRepeatedOutlier_ShouldDisableIt_AndSwitch()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<ExtPriceExchangesController>().AsSelf());
            var container = env.CreateContainer();
            container.ChangeExchangeSettings("BTCUSD", "bitmex", m => m.Hedging.DefaultPreference = 1);
            var marketMakerService = container.Resolve<IMarketMakerService>();

            //act
            var bitmexDecimals = Generate.Decimals(1.01m); // note outlier threshold 5%
            var firstExpectedBatch = env.GetExpectedCommandsBatch("BTCUSD", bitmexDecimals);
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex", bitmexDecimals));
            await marketMakerService.ProcessNewExternalOrderbookAsync(
                env.GetInpMessage("Poloniex", Generate.Decimals()));
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", Generate.Decimals()));

            var bitmexBadDecimals = Generate.Decimals(1.05m); // note outlier threshold 5%
            for (var i = 0; i < 11; i++) // note repeated outlier sequence threshold is 10
            {
                env.Sleep(new TimeSpan(1)); // timestamps should be different for repeated outlier functionality to work
                await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("bitmex",
                    bitmexBadDecimals.Clone()));
            }

            env.SleepSecs(2);
            var krakenDecimals = Generate.Decimals(1.01m);
            await marketMakerService.ProcessNewExternalOrderbookAsync(env.GetInpMessage("Kraken", krakenDecimals));

            //assert
            env.PrintLogs();
            env.VerifyMessagesSentWithTime(
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "bitmex"),
                firstExpectedBatch,
                env.GetExpectedPrimaryExchangeMessage("BTCUSD", "Kraken"),
                env.GetExpectedTradesControls("BTCUSD", true),
                env.GetExpectedCommandsBatch("BTCUSD", krakenDecimals));
        }
    }
}