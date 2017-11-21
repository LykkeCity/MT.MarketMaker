using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.Common;
using MarginTrading.MarketMaker.Services.Common.Implementation;
using MarginTrading.MarketMaker.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.MarketMaker.Modules
{
    internal class MarketMakerModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        private readonly IServiceCollection _services = new ServiceCollection();

        public MarketMakerModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterDefaultImplementations(builder);

            builder.RegisterInstance(_settings.Nested(s => s.MarginTradingMarketMaker)).SingleInstance();
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemService>().As<ISystem>().SingleInstance();

            builder.RegisterInstance(new RabbitMqService(_log,
                    _settings.Nested(s => s.MarginTradingMarketMaker.Db.QueuePersistanceRepositoryConnString)))
                .As<IRabbitMqService>().SingleInstance();

            builder.RegisterType<BrokerService>().As<IBrokerService>().InstancePerDependency();

            _services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.MarginTradingMarketMaker.ExternalServices.AssetsServiceUrl),
                TimeSpan.FromMinutes(5)));
            builder.RegisterInstance(
                    new Candleshistoryservice(new Uri(_settings.CurrentValue.CandlesHistoryServiceClient.ServiceUrl)))
                .As<ICandleshistoryservice>()
                .SingleInstance();

            builder.Populate(_services);
        }

        /// <summary>
        /// Scans for types in the current assembly and registers types which: <br/>
        /// - are named like 'SmthService' <br/>
        /// - implement an non-generic interface named like 'ISmthService' in the same assembly <br/>
        /// - are the only implementations of the 'ISmthService' interface <br/>
        /// - are not generic <br/><br/>
        /// Types like SmthRepository are also supported.
        /// Also autoregisters <see cref="IStartable"/>'s.
        /// </summary>
        private void RegisterDefaultImplementations(ContainerBuilder builder)
        {
            var assembly = GetType().Assembly;
            var implementations = assembly.GetTypes()
                .Where(t => !t.IsInterface && !t.IsGenericType && (t.Name.EndsWith("Service") || t.Name.EndsWith("Repository")))
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i => i.Name.StartsWith('I') && i.Assembly == assembly || i == typeof(IStartable))
                        .Select(i => (Implementation: t, Interface: i)))
                .GroupBy(t => t.Interface)
                .Where(gr => gr.Count() == 1 || gr.Key == typeof(IStartable))
                .SelectMany(gr => gr)
                .GroupBy(t => t.Implementation, t => t.Interface);

            foreach (var gr in implementations)
            {
                builder.RegisterType(gr.Key).As(gr.ToArray()).SingleInstance();
            }
        }
    }
}