using System;
using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class SettingsRoot
    {
        public ImmutableSortedDictionary<string, AssetPairSettings> AssetPairs { get; }

        public SettingsRoot(ImmutableSortedDictionary<string, AssetPairSettings> assetPairs)
        {
            AssetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
        }
    }
}
