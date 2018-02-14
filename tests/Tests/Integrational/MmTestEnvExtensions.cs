using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using FluentAssertions.Equivalency;
using MarginTrading.MarketMaker.AzureRepositories.Implementation;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Messages;
using MoreLinq;
using Newtonsoft.Json;

namespace Tests.Integrational
{
    internal static class MmTestEnvExtensions
    {
        public static void VerifyMessagesSent(this IMmTestEnvironment testEnvironment, params object[] messages)
        {
            VerifyMessagesSentCore(testEnvironment, messages, o => o
                .Excluding(i => i.SelectedMemberPath.EndsWith(".Timestamp", StringComparison.OrdinalIgnoreCase)));
        }

        public static void VerifyMessagesSentWithTime(this IMmTestEnvironment testEnvironment, params object[] messages)
        {
            VerifyMessagesSentCore(testEnvironment, messages, o => o);
        }

        public static StopOrAllowNewTradesMessage GetExpectedTradesControls(this IMmTestEnvironment testEnvironment,
            string assetPairId, bool isStopped)
        {
            return new StopOrAllowNewTradesMessage
            {
                AssetPairId = assetPairId,
                MarketMakerId = "testMmId",
                Stop = isStopped,
            };
        }

        public static StartedMessage GetStartedMessage(this IMmTestEnvironment testEnvironment)
        {
            return new StartedMessage {MarketMakerId = "testMmId"};
        }

        public static PrimaryExchangeSwitchedMessage GetExpectedPrimaryExchangeMessage(
            this IMmTestEnvironment testEnvironment,
            string assetPairId, string exchange)
        {
            return new PrimaryExchangeSwitchedMessage
            {
                AssetPairId = assetPairId,
                MarketMakerId = "testMmId",
                NewPrimaryExchange = new ExchangeQualityMessage {ExchangeName = exchange},
            };
        }

        public static void PrintLogs(this IMmTestEnvironment testEnvironment)
        {
            Console.WriteLine(JsonConvert.SerializeObject(Trace.TraceService.GetLast(), Formatting.Indented));
        }

        public static DateTime Sleep(this IMmTestEnvironment env, TimeSpan time)
        {
            return env.UtcNow += time;
        }

        public static DateTime SleepSecs(this IMmTestEnvironment env, double seconds)
        {
            return env.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public static ExchangeExtPriceSettingsModel GetExchangeSettings(this IContainer container, string assetPairId,
            string exchangeName)
        {
            return container.Resolve<ExtPriceExchangesController>().Get(assetPairId, exchangeName)
                .RequiredNotNull("settings");
        }
        
        public static IReadOnlyList<ExtPriceStatusModel> GetStatus(this IContainer container, string assetPairId)
        {
            return container.Resolve<ExtPriceStatusController>().Get(assetPairId).RequiredNotNull("settings");
        }

        public static void ChangeExchangeSettings(this IContainer container, string assetPairId, string exchangeName,
            Action<ExchangeExtPriceSettingsModel> update)
        {
            var extPriceExchangesController = container.Resolve<ExtPriceExchangesController>();
            var settings = extPriceExchangesController.Get(assetPairId, exchangeName).RequiredNotNull("settings");
            update(settings);
            extPriceExchangesController.Update(settings);
        }

        public static void ChangeAssetSettings(this IContainer container, string assetPairId,
            Action<AssetPairExtPriceSettingsModel> update)
        {
            var extPriceExchangesController = container.Resolve<ExtPriceSettingsController>();
            var settings = extPriceExchangesController.Get(assetPairId).RequiredNotNull("settings");
            update(settings);
            extPriceExchangesController.Update(settings);
        }

        public static ExternalExchangeOrderbookMessage GetInpMessage(this IMmTestEnvironment testEnvironment,
            string exchangeName, Generator<decimal> decimals)
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
                Timestamp = testEnvironment.UtcNow,
            };
        }

        public static OrderCommandsBatchMessage GetExpectedCommandsBatch(this IMmTestEnvironment testEnvironment,
            string assetPairId,
            Generator<decimal> generator)
        {
            generator = generator.Clone();
            var commands = new List<OrderCommand>
            {
                new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder}
            };

            commands.AddRange(GetCommands(generator.Take(4), OrderDirectionEnum.Buy).OrderByDescending(e => e.Price));
            commands.AddRange(GetCommands(generator.Take(4), OrderDirectionEnum.Sell).OrderBy(e => e.Price));

            return new OrderCommandsBatchMessage
            {
                AssetPairId = assetPairId,
                Commands = commands,
                MarketMakerId = "testMmId",
                Timestamp = testEnvironment.UtcNow,
            };
        }

        private static void VerifyMessagesSentCore(this IMmTestEnvironment testEnvironment,
            IEnumerable<object> messages,
            Func<EquivalencyAssertionOptions<object>, EquivalencyAssertionOptions<object>> config)
        {
            var sent = testEnvironment.StubRabbitMqService.GetSentMessages();
            sent.Should().BeEquivalentTo(messages,
                o => config(o.WithStrictOrdering().Using<object>(CustomCompare).When(s => true)));
        }

        private static void CustomCompare(IAssertionContext<object> a)
        {
            if (a.Expectation != null)
            {
                if (a.Expectation is ExchangeQualityMessage msgExp
                    && a.Subject is ExchangeQualityMessage msgSubj)
                {
                    msgSubj.ExchangeName.Should().Be(msgExp.ExchangeName);
                }
                else
                {
                    a.Subject.Should().BeEquivalentTo(a.Expectation,
                        o => o.WithStrictOrdering().Using<object>(CustomCompare).When(s => true));
                }
            }
        }

        private static IEnumerable<OrderCommand> GetCommands(IEnumerable<decimal> src,
            OrderDirectionEnum orderDirection)
        {
            return src.Batch(2, d => d.ToList()).Select(l => new OrderCommand
            {
                CommandType = OrderCommandTypeEnum.SetOrder,
                Direction = orderDirection,
                Price = l.First(),
                Volume = l.Last()
            });
        }
    }
}