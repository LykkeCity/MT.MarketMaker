using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class SettingsRootStorageModel
    {
        public int Version => 1;
        
        public ImmutableDictionary<string, AssetPairSettingsStorageModel> AssetPairs
            = ImmutableDictionary<string, AssetPairSettingsStorageModel>.Empty;
    }
}