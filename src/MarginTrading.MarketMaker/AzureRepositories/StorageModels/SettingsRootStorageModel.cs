using System.Collections.Immutable;

namespace MarginTrading.MarketMaker.AzureRepositories.StorageModels
{
    internal class SettingsRootStorageModel
    {
        public int Version { get; set; }
        
        public ImmutableSortedDictionary<string, AssetPairSettingsStorageModel> AssetPairs
            = ImmutableSortedDictionary<string, AssetPairSettingsStorageModel>.Empty;
    }
}