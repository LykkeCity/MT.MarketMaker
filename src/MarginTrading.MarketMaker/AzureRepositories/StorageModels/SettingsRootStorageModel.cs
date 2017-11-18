using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class SettingsRootStorageModel
    {
        public ImmutableDictionary<string, AssetPairSettingsStorageModel> AssetPairs
            = ImmutableDictionary<string, AssetPairSettingsStorageModel>.Empty;
    }
}