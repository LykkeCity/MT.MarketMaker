using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class SettingsRootService : IStartable, ISettingsRootService
    {
        private readonly ISettingsStorage _settingsStorage;
        private SettingsRoot _cache;

        public SettingsRootService(ISettingsStorage settingsStorage)
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