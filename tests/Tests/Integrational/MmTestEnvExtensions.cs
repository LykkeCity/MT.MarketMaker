using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
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
            testEnvironment.StubRabbitMqService.GetSentMessages<OrderCommandsBatchMessage>()
                .ShouldAllBeEquivalentTo(
                    testEnvironment.GetExpectedCommands(pairsData));
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
                    }), o => o.ExcludingMissingMembers());
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
                    }), o => o.ExcludingMissingMembers());
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

                commands.AddRange(GetCommands(bids, OrderDirectionEnum.Buy));
                commands.AddRange(GetCommands(asks, OrderDirectionEnum.Sell));

                yield return new OrderCommandsBatchMessage
                {
                    AssetPairId = assetPairId,
                    Commands = commands,
                    MarketMakerId = "testMmId",
                    Timestamp = testEnvironment.UtcNow,
                };
            }
        }

        public static void PrintLogs(this IContainer container)
        {
            Console.WriteLine(JsonConvert.SerializeObject(container.Resolve<ITraceService>().GetLast(), Formatting.Indented));
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