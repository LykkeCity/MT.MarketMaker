using System;
using System.Collections.Immutable;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class SettingsRootService : IStartable, ISettingsRootService
    {
        private readonly ISettingsStorageService _settingsStorageService;
        [CanBeNull] private SettingsRoot _cache;
        private static readonly object _updateLock = new object();

        public SettingsRootService(ISettingsStorageService settingsStorageService)
        {
            _settingsStorageService = settingsStorageService;
        }

        public SettingsRoot Get()
        {
            return _cache.RequiredNotNull("_cache != null");
        }

        public AssetPairSettings Get(string assetPairId)
        {
            return Get().AssetPairs.GetValueOrDefault(assetPairId);
        }

        public void Set([NotNull] SettingsRoot settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            lock (_updateLock)
            {
                _settingsStorageService.Write(settings);
                _cache = settings;
            }
        }

        public void Update([NotNull] string assetPairId, [NotNull] Func<AssetPairSettings, AssetPairSettings> changeFunc)
        {
            if (changeFunc == null) throw new ArgumentNullException(nameof(changeFunc));
            Change(old =>
            {
                var assetPairSettings = old.AssetPairs.GetValueOrDefault(assetPairId)
                                        ?? throw new ArgumentException($"Settings for {assetPairId} not found",
                                            nameof(assetPairId));
                return new SettingsRoot(old.AssetPairs.SetItem(assetPairId, changeFunc(assetPairSettings)));
            });
        }

        public void Delete([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            Change(old =>
            {
                var newSettings = old.AssetPairs.Remove(assetPairId);
                if (newSettings == old.AssetPairs)
                {
                    throw new ArgumentException($"Settings for {assetPairId} not found",
                        nameof(assetPairId));
                }
                
                return new SettingsRoot(newSettings);
            });
        }

        public void Add(string assetPairId, [NotNull] AssetPairSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Change(old => new SettingsRoot(old.AssetPairs.Add(assetPairId, settings)));
        }

        private void Change(Func<SettingsRoot, SettingsRoot> changeFunc)
        {
            lock (_updateLock)
            {
                var oldSettings = Get();
                var settings = changeFunc(oldSettings);
                _settingsStorageService.Write(settings);
                _cache = settings;
            }
        }

        public void Start()
        {
            _cache = _settingsStorageService.Read()
                ?? new SettingsRoot(ImmutableDictionary<string, AssetPairSettings>.Empty);
        }
    }
}