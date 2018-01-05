using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Enums;
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
            var sent = testEnvironment.StubRabbitMqService.GetSentMessages();
            sent.ShouldContainEquivalentInOrder(messages,
                o => o.WithStrictOrdering().ExcludingMissingMembers().Excluding(info =>
                    info.SelectedMemberPath.EndsWith(".Timestamp", StringComparison.OrdinalIgnoreCase)));
        }
        
        public static void VerifyMessagesSentWithTime(this IMmTestEnvironment testEnvironment, params object[] messages)
        {
            var sent = testEnvironment.StubRabbitMqService.GetSentMessages();
            sent.ShouldContainEquivalentInOrder(messages, o => o.WithStrictOrdering().ExcludingMissingMembers());
        }

        public static object GetExpectedTradesControls(this IMmTestEnvironment testEnvironment,
            string assetPairId, bool isStopped)
        {
            return new
            {
                AssetPairId = assetPairId,
                MarketMakerId = "testMmId",
                Stop = isStopped,
            };
        }
        
        public static object GetExpectedPrimaryExchangeMessage(this IMmTestEnvironment testEnvironment,
            string assetPairId, string exchange)
        {
            return new
            {
                AssetPairId = assetPairId,
                MarketMakerId = "testMmId",
                NewPrimaryExchange = new {ExchangeName = exchange},
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

        public static ExternalExchangeOrderbookMessage GetInpMessage(this IMmTestEnvironment testEnvironment, string exchangeName, Generator<decimal> decimals)
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

        public static OrderCommandsBatchMessage GetExpectedCommandsBatch(this IMmTestEnvironment testEnvironment, string assetPairId,
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