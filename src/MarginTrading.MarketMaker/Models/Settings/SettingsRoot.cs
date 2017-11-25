using System;
using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class SettingsRoot
    {
        public ImmutableDictionary<string, AssetPairSettings> AssetPairs { get; }

        public SettingsRoot(ImmutableDictionary<string, AssetPairSettings> assetPairs)
        {
            AssetPairs = assetPairs ?? throw new ArgumentNullException(nameof(assetPairs));
        }
    }
}
