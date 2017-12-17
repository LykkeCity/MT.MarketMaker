using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Service.Assets.Client;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.MarketMaker.Modules
{
    internal class MarketMakerExternalServicesModule: Module
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly IReloadingManager<AppSettings> _settings;

        public MarketMakerExternalServicesModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.MarginTradingMarketMaker.ExternalServices.AssetsServiceUrl),
                TimeSpan.FromMinutes(5)));
            
            builder.RegisterInstance(
                    new Candleshistoryservice(new Uri(_settings.CurrentValue.CandlesHistoryServiceClient.ServiceUrl)))
                .As<ICandleshistoryservice>()
                .SingleInstance();
            
            builder.Populate(_services);
        }
    }
}