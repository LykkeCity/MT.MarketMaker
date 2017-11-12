﻿namespace MarginTrading.MarketMaker.Enums
{
    /// <summary>
    /// The quotes source for the asset pair
    /// </summary>
    public enum AssetPairQuotesSourceTypeEnum
    {
        /// <summary>
        /// Quotes are provided only manually - used for test purposes
        /// </summary>
        Manual = 1,

        /// <summary>
        /// Quotes are provided only from external exchanges
        /// </summary>
        External = 2,

        /// <summary>
        /// Quotes are provided from the spot orderbook
        /// </summary>
        Spot = 3,

        /// <summary>
        /// Quotes are provided using cross-rates from external exchanges
        /// </summary>
        CrossRates = 4,
    }
}