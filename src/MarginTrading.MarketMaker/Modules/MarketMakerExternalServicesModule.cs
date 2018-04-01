using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Settings;
using Microsoft.ApplicationInsights.Extensibility;
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
            _services.RegisterMtDataReaderClient(_settings.CurrentValue.MtDataReaderLiveServiceClient.ServiceUrl,
                _settings.CurrentValue.MtDataReaderLiveServiceClient.ApiKey, "MarginTrading.MarketMaker");
            
            builder.RegisterInstance(
                    new Candleshistoryservice(new Uri(_settings.CurrentValue.CandlesHistoryServiceClient.ServiceUrl)))
                .As<ICandleshistoryservice>()
                .SingleInstance();
            
            _services.AddSingleton<ITelemetryInitializer, UserAgentTelemetryInitializer>();
            
            builder.Populate(_services);
        }
    }
}