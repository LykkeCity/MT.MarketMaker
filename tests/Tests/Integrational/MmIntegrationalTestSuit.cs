﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.CandlesHistory.Client;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Modules;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MarginTrading.MarketMaker.Settings;
using Moq;
using Trace = MarginTrading.MarketMaker.Infrastructure.Implementation.Trace;

namespace Tests.Integrational
{
    internal class MmIntegrationalTestSuit : IntegrationalTestSuit
    {
        private static readonly LogToConsole LogToConsole = new LogToConsole();

        public AppSettings AppSettings { get; set; } = new AppSettings
        {
            MarginTradingMarketMaker = new MarginTradingMarketMakerSettings
            {
                MarketMakerId = "testMmId",
                Db = new DbSettings {QueuePersistanceRepositoryConnString = "fake"}
            }
        };

        public MmIntegrationalTestSuit()
        {
            WithModule(new MarketMakerModule(
                new StubReloadingManager<AppSettings>(() => AppSettings), LogToConsole));
        }

        public new IMmTestEnvironment Build()
        {
            return (IMmTestEnvironment) base.Build();
        }

        protected override TestEnvironment GetTestContainerBuilder()
        {
            return new MmTestEnvironment(this);
        }

        private class MmTestEnvironment : TestEnvironment, IMmTestEnvironment
        {
            public DateTime UtcNow { get; set; } = DateTime.UtcNow;
            public StubRabbitMqService StubRabbitMqService { get; } = new StubRabbitMqService();

            public IList<AssetPairResponseModel> AssetPairs { get; set; } = new[]
            {
                new AssetPairResponseModel
                {
                    BaseAssetId = "BTC",
                    Id = "BTCUSD",
                    QuotingAssetId = "USD",
                    Source = "",
                    Source2 = ""
                }
            };

            public SettingsRoot SettingsRoot { get; set; } = new SettingsRoot(
                ImmutableDictionary<string, AssetPairSettings>.Empty.Add("BTCUSD",
                    new AssetPairSettings(AssetPairQuotesSourceTypeEnum.External,
                        GetDefaultExtPriceSettings(), GetDefaultCrossRateCalcInfo("BTCUSD"))));

            public MmTestEnvironment(MmIntegrationalTestSuit suit) : base(suit)
            {
                Setup<ISettingsStorageService>(
                        m => m.Setup(s => s.Read()).Returns(() => SettingsRoot),
                        m => m.Setup(s => s.Write(It.IsNotNull<SettingsRoot>()))
                            .Callback<SettingsRoot>(r => SettingsRoot = r))
                    .Setup<IRabbitMqService>(StubRabbitMqService)
                    .Setup<ISystem>(m => m.Setup(s => s.UtcNow).Returns(() => UtcNow))
                    .Setup<IAssetsService>(m => m.Setup(s => s.GetAssetPairsWithHttpMessagesAsync(default, default))
                        .Returns(() => AssetPairs.ToResponse()))
                    .Setup(new Mock<IMtMmRisksSlackNotificationsSender>().Object)
                    .Setup<ILog>(LogToConsole)
                    .Setup<ICandleshistoryservice>();
            }

            public override IContainer CreateContainer()
            {
                var container = base.CreateContainer();
                Trace.TraceService = container.Resolve<ITraceService>();
                return container;
            }
        }

        private static AssetPairExtPriceSettings GetDefaultExtPriceSettings()
        {
            return new AssetPairExtPriceSettings("bitmex",
                0.05m, TimeSpan.Zero, new AssetPairMarkupsParams(0, 0),
                new RepeatedOutliersParams(10, TimeSpan.FromMinutes(5), 10, TimeSpan.FromMinutes(5)),
                Enum.GetValues(typeof(OrderbookGeneratorStepEnum)).Cast<OrderbookGeneratorStepEnum>()
                    .ToImmutableDictionary(e => e, e => true),
                ImmutableDictionary<string, ExchangeExtPriceSettings>.Empty
                    .Add("bitmex", GetDefaultExtPriceExchangeSettings())
                    .Add("bitfinex", GetDefaultExtPriceExchangeSettings())
                    .Add("Poloniex", GetDefaultExtPriceExchangeSettings())
                    .Add("Kraken", GetDefaultExtPriceExchangeSettings()));
        }

        private static ExchangeExtPriceSettings GetDefaultExtPriceExchangeSettings()
        {
            return new ExchangeExtPriceSettings(TimeSpan.FromSeconds(30), new ExchangeDisabledSettings(false, ""),
                new ExchangeHedgingSettings(0, false),
                new ExchangeOrderGenerationSettings(1, TimeSpan.FromSeconds(10)));
        }

        private static CrossRateCalcInfo GetDefaultCrossRateCalcInfo(string assetPairId)
        {
            return new CrossRateCalcInfo(assetPairId, new CrossRateSourceAssetPair(string.Empty, false),
                new CrossRateSourceAssetPair(string.Empty, false));
        }
    }
}