using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Assets.Client.Custom;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services;
using MarginTrading.MarketMaker.Services.Implementation;
using MarginTrading.MarketMaker.Settings;
using Microsoft.Extensions.DependencyInjection;
using Rocks.Caching;

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
            builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();

            builder.RegisterInstance(new RabbitMqService(_log,
                    _settings.Nested(s => s.MarginTradingMarketMaker.Db.QueuePersistanceRepositoryConnString)))
                .As<IRabbitMqService>().SingleInstance();

            builder.RegisterType<BrokerService>().As<IBrokerService>().InstancePerDependency();

            _services.UseAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.MarginTradingMarketMaker.ExternalServices.AssetsServiceUrl),
                TimeSpan.FromMinutes(5)));

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
                .ToList();

            foreach (var (impl, service) in implementations)
            {
                builder.RegisterType(impl).As(service).SingleInstance();
            }
        }
    }
}