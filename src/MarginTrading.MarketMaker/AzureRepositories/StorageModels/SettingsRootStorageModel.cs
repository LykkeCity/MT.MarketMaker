using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class SettingsRootStorageModel
    {
        public int Version => 1;
        
        public ImmutableSortedDictionary<string, AssetPairSettingsStorageModel> AssetPairs
            = ImmutableSortedDictionary<string, AssetPairSettingsStorageModel>.Empty;
    }
}