using System.Collections.Generic;
using System.Linq;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Messages;
using MoreLinq;

namespace Tests.Integrational
{
    internal static class MmTestContainerBuilderExtensions
    {
        public static IEnumerable<OrderCommandsBatchMessage> GetExpectedCommands(
            this IMmTestContainerBuilder testContainerBuilder,
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
                    Timestamp = testContainerBuilder.UtcNow,
                };
            }
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