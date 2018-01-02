using MarginTrading.MarketMaker.Contracts.Enums;

namespace MarginTrading.MarketMaker.Contracts.Messages
{
    /// <summary>
    /// A command to set or delete an order. The AssetPairId is specified in the <see cref="OrderCommandsBatchMessage"/>
    /// </summary>
    public class OrderCommand
    {
        /// <summary>
        /// What to do with the order - set or delete
        /// </summary>
        public OrderCommandTypeEnum CommandType { get; set; }

        /// <summary>
        /// The order should be to buy or to sell. Null is used to remove orders with any direction.
        /// </summary>
        public OrderDirectionEnum? Direction { get; set; }

        /// <summary>
        /// Order volume. Null is used to remove orders with any volume.
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Order price. Null is used to remove orders with any price.
        /// </summary>
        public decimal? Price { get; set; }
    }
}
