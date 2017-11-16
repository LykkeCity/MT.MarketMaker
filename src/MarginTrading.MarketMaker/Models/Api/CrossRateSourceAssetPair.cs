namespace MarginTrading.MarketMaker.Models.Api
{
    public class CrossRateSourceAssetPairModel
    {
        /// <summary>
        /// Asset pair Id, ex. BTCUSD
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// True if the base asset is quoting in the pair.
        /// </summary>
        /// <remarks>
        /// Base asset is the one that is common in two source pairs used for cross-rate calculating.<br/>
        /// Ex: ETHUSD is calculated based on BTC from ETHBTC (will have this prop = true) and BTCUSD (will have false).
        /// </remarks>
        public bool IsTransitoryAssetQuoting { get; set; }
    }
}