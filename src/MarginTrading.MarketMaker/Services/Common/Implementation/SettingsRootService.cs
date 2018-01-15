using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class SettingsRootService : ICustomStartup, ISettingsRootService
    {
        private readonly ISettingsStorageService _settingsStorageService;
        private readonly ISettingsValidationService _settingsValidationService;
        private readonly ISettingsChangesAuditService _settingsChangesAuditService;
        private readonly ISettingsChangesAuditRepository _settingsChangesAuditRepository;

        [CanBeNull] private SettingsRoot _cache;
        private static readonly object _updateLock = new object();

        public SettingsRootService(ISettingsStorageService settingsStorageService,
            ISettingsValidationService settingsValidationService,
            ISettingsChangesAuditService settingsChangesAuditService,
            ISettingsChangesAuditRepository settingsChangesAuditRepository)
        {
            _settingsStorageService = settingsStorageService;
            _settingsValidationService = settingsValidationService;
            _settingsChangesAuditService = settingsChangesAuditService;
            _settingsChangesAuditRepository = settingsChangesAuditRepository;
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
            lock (_updateLock) WriteUnsafe(Get(), settings);
        }

        public void Update([NotNull] string assetPairId,
            [NotNull] Func<AssetPairSettings, AssetPairSettings> changeFunc)
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
                WriteUnsafe(oldSettings, settings);
            }
        }

        private void WriteUnsafe(SettingsRoot oldSettings, SettingsRoot settings)
        {
            _settingsValidationService.Validate(settings);
            var audit = _settingsChangesAuditService.GetAudit(oldSettings, settings);
            if (audit == null) return; // nothing changed
            
            _settingsChangesAuditRepository.Insert(audit);
            _settingsStorageService.Write(settings);
            _cache = settings;
        }

        public void Initialize()
        {
            var settingsRoot = _settingsStorageService.Read()
                               ?? new SettingsRoot(ImmutableSortedDictionary<string, AssetPairSettings>.Empty);
            _settingsValidationService.Validate(settingsRoot);
            _cache = settingsRoot;
        }
    }
}