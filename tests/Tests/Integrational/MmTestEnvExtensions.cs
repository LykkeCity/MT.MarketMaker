using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Controllers;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Messages;
using MoreLinq;
using Newtonsoft.Json;

namespace Tests.Integrational
{
    internal static class MmTestEnvExtensions
    {
        public static void VerifyCommandsSent(
            this IMmTestEnvironment testEnvironment,
            params (string AssetPairId, IEnumerable<decimal> Bids, IEnumerable<decimal> Asks)[] pairsData)
        {
            var sent = testEnvironment.StubRabbitMqService.GetSentMessages<OrderCommandsBatchMessage>();
            var expected = testEnvironment.GetExpectedCommands(pairsData);
            sent.ShouldAllBeEquivalentTo(expected, o => o.WithStrictOrdering().Excluding(c => c.Timestamp));
        }

        public static void VerifyCommandsSent(
            this IMmTestEnvironment testEnvironment,
            params (string AssetPairId, Generator<decimal> Generator)[] pairsData)
        {
            testEnvironment.VerifyCommandsSent(pairsData.Select(p =>
            {
                var g = p.Generator.Clone();
                return (p.AssetPairId, g.Take(4), g.Take(4));
            }).ToArray());
        }

        public static void VerifyTradesStopped(this IMmTestEnvironment testEnvironment, string assetPairId, params bool[] isStopped)
        {
            testEnvironment.StubRabbitMqService.GetSentMessages<StopOrAllowNewTradesMessage>()
                .ShouldAllBeEquivalentTo(
                    isStopped.Select(s => new
                    {
                        AssetPairId = assetPairId,
                        MarketMakerId = "testMmId",
                        Stop = s,
                    }), o => o.WithStrictOrdering().ExcludingMissingMembers());
        }
        
        public static void VerifyPrimaryExchangeSwitched(this IMmTestEnvironment testEnvironment, string assetPairId, params string[] exchangeNames)
        {
            testEnvironment.StubRabbitMqService.GetSentMessages<PrimaryExchangeSwitchedMessage>()
                .ShouldAllBeEquivalentTo(
                    exchangeNames.Select(e => new
                    {
                        AssetPairId = assetPairId,
                        MarketMakerId = "testMmId",
                        NewPrimaryExchange = new { ExchangeName = e },
                    }), o => o.WithStrictOrdering().ExcludingMissingMembers());
        }
        
        public static IEnumerable<OrderCommandsBatchMessage> GetExpectedCommands(
            this IMmTestEnvironment testEnvironment,
            params (string AssetPairId, IEnumerable<decimal> Bids, IEnumerable<decimal> Asks)[] pairsData)
        {
            foreach (var (assetPairId, bids, asks) in pairsData)
            {
                var commands = new List<OrderCommand>
                {
                    new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder}
                };

                commands.AddRange(GetCommands(bids, OrderDirectionEnum.Buy).OrderByDescending(e => e.Price));
                commands.AddRange(GetCommands(asks, OrderDirectionEnum.Sell).OrderBy(e => e.Price));

                yield return new OrderCommandsBatchMessage
                {
                    AssetPairId = assetPairId,
                    Commands = commands,
                    MarketMakerId = "testMmId",
                    Timestamp = testEnvironment.UtcNow,
                };
            }
        }

        public static void PrintLogs(this IMmTestEnvironment testEnvironment)
        {
            Console.WriteLine(JsonConvert.SerializeObject(Trace.TraceService.GetLast(), Formatting.Indented));
        }
        
        public static void Sleep(this IMmTestEnvironment env, TimeSpan time)
        {
            env.UtcNow += time;
        }
        
        public static void SleepSecs(this IMmTestEnvironment env, double seconds)
        {
            env.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public static void ChangeExchangeSettings(this IContainer container, string assetPairId, string exchangeName, Action<ExchangeExtPriceSettingsModel> update)
        {
            var extPriceExchangesController = container.Resolve<ExtPriceExchangesController>();
            var settings = extPriceExchangesController.Get(assetPairId, exchangeName).RequiredNotNull("settings");
            update(settings);
            extPriceExchangesController.Update(settings);
        }

        public static void ChangeAssetSettings(this IContainer container, string assetPairId, Action<AssetPairExtPriceSettingsModel> update)
        {
            var extPriceExchangesController = container.Resolve<ExtPriceSettingsController>();
            var settings = extPriceExchangesController.Get(assetPairId).RequiredNotNull("settings");
            update(settings);
            extPriceExchangesController.Update(settings);
        }

        private static IEnumerable<OrderCommand> GetCommands(IEnumerable<decimal> src, OrderDirectionEnum orderDirection)
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