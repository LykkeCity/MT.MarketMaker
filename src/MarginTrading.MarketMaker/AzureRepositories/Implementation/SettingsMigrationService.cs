using System;
using System.Collections.Immutable;
using System.Linq;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MoreLinq;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsMigrationService : ISettingsMigrationService
    {
        static readonly ImmutableSortedDictionary<int, Action<SettingsRootStorageModel>>
            _migrations = ImmutableSortedDictionary<int, Action<SettingsRootStorageModel>>.Empty
            .Add(2, MigrateTo2);

        public void Migrate(SettingsRootStorageModel model)
        {
            _migrations.Where(p => p.Key > model.Version)
                .ForEach(p => p.Value.Invoke(model));
        }

        private static void MigrateTo2(SettingsRootStorageModel model)
        {
            foreach (var assetPair in model.AssetPairs)
            {
                assetPair.Value.AggregateOrderbookSettings.RequiredEqualsTo(null,
                    $"model.AssetPairs[{assetPair.Key}].AggregateOrderbookSettings");
                assetPair.Value.AggregateOrderbookSettings = new AggregateOrderbookSettingsStorageModel
                {
                    AsIsLevelsCount = 0,
                    CumulativeVolumeLevels = ImmutableSortedSet<decimal>.Empty,
                    RandomFraction = 0.05m,
                };
                assetPair.Value.ExtPriceSettings.Steps = assetPair.Value.ExtPriceSettings.Steps
                    .SetItem(OrderbookGeneratorStepDomainEnum.AggregateOrderbook, false);
            }
        }
    }
}