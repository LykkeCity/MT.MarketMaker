using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Settings.Implementation
{
    internal class SettingsService : IStartable, ISettingsService
    {
        private readonly ISettingsStorage _settingsStorage;
        private SettingsRoot _cache;

        public SettingsService(ISettingsStorage settingsStorage)
        {
            _settingsStorage = settingsStorage;
        }

        public Task Set([NotNull] SettingsRoot settings)
        {
            _settingsStorage.Set(settings);
            _cache = settings;
        }

        public SettingsRoot Get()
        {
            return _cache.RequiredNotNull("_cache != null");
        }

        public void Start()
        {
            _cache = _settingsStorage.Get().GetAwaiter().GetResult();
        }
    }
}