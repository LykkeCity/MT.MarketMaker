using System;
using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Settings
{
    internal class SettingsRoot
    {
        public ImmutableDictionary<string, AssetPairSettings> AssetPairs;

        public SettingsRoot(ImmutableDictionary<string, AssetPairSettings> assetPairs)
        {
            AssetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
        }
    }
}
