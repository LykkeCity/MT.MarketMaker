﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Logs;
using Lykke.Service.CandlesHistory.Client;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.AzureRepositories.Implementation;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Modules;
using MarginTrading.MarketMaker.Settings;
using Moq;

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
                Db = new DbSettings {QueuePersistanceRepositoryConnString = "fake"},
               LegalEntity = "LYKKEVU"
            },
            RiskInformingSettings = new RiskInformingSettings {Data = new []
            {
                new RiskInformingParams
                {
                    EventTypeCode = "MM01",
                    Level = "None",
                    System = "MarketMaker",
                },
                new RiskInformingParams
                {
                    EventTypeCode = "MM02",
                    Level = "None",
                    System = "MarketMaker",
                },
                new RiskInformingParams
                {
                    EventTypeCode = "MM03",
                    Level = "None",
                    System = "MarketMaker",
                },
                new RiskInformingParams
                {
                    EventTypeCode = "MM04",
                    Level = "None",
                    System = "MarketMaker",
                },
                new RiskInformingParams
                {
                    EventTypeCode = "MM05",
                    Level = "None",
                    System = "MarketMaker",
                },
            }}
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
            public InMemoryTableStorageFactory TableStorageFactory { get; } = new InMemoryTableStorageFactory();
            public InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; } =
                new InMemoryBlobStorageSingleObjectFactory();
            public TestRandom Random { get; } = new TestRandom();

            public List<AssetPairContract> AssetPairs { get; set; } = new List<AssetPairContract>
            {
                new AssetPairContract
                {
                    Id = "BTCUSD",
                    Accuracy = 3,
                }
            };

            public SettingsRootStorageModel SettingsRoot
            {
                get => BlobStorageFactory.Blob.GetObject<SettingsRootStorageModel>();
                set => BlobStorageFactory.Blob.Object = value;
            }

            public MmTestEnvironment(MmIntegrationalTestSuit suit) : base(suit)
            {
                Setup<IRabbitMqService>(StubRabbitMqService)
                    .Setup<ISystem>(m => m.Setup(s => s.UtcNow).Returns(() => UtcNow),
                        m => m.Setup(s => s.GetRandom()).Returns(Random))
                    .Setup<IMtDataReaderClient>(c => c.AssetPairsRead == suit.GetMockObj<IAssetPairsReadingApi>())
                    .Setup<IAssetPairsReadingApi>(m => m.Setup(s => s.List("LYKKEVU", MatchingEngineModeContract.MarketMaker))
                        .ReturnsAsync(() => AssetPairs))
                    .Setup<ILog>(LogToConsole)
                    .Setup<ICandleshistoryservice>()
                    .Setup(new LykkeLogToAzureStorage(null))
                    .Setup<ISlackNotificationsSender>(s =>
                        s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) == Task.CompletedTask)
                    .Setup<IAzureTableStorageFactoryService>(TableStorageFactory)
                    .Setup<IAzureBlobStorageFactoryService>(BlobStorageFactory);
            }

            public override IContainer CreateContainer()
            {
                SettingsRoot = GetDefaultSettingsRoot();
                var container = base.CreateContainer();
                Trace.TraceService = container.Resolve<ITraceService>();
                return container;
            }

            private static SettingsRootStorageModel GetDefaultSettingsRoot()
            {
                return new SettingsRootStorageModel
                {
                    AssetPairs = ImmutableSortedDictionary<string, AssetPairSettingsStorageModel>.Empty.Add("BTCUSD",
                        new AssetPairSettingsStorageModel
                        {
                            QuotesSourceType = AssetPairQuotesSourceTypeDomainEnum.External,
                            AggregateOrderbookSettings = GetDefaultAggregateOrderbookSettings(),
                            CrossRateCalcInfo = GetDefaultCrossRateCalcInfo("BTCUSD"),
                            ExtPriceSettings = GetDefaultExtPriceSettings(),
                        }),
                    Version = SettingsStorageService.CurrentStorageModelVersion,
                };
            }
        }

        private static AssetPairExtPriceSettingsStorageModel GetDefaultExtPriceSettings()
        {
            return new AssetPairExtPriceSettingsStorageModel
            {
                PresetDefaultExchange = "bitmex",
                OutlierThreshold = 0.05m,
                MinOrderbooksSendingPeriod = TimeSpan.Zero,
                Markups = new AssetPairExtPriceSettingsStorageModel.MarkupsParamsStorageModel(),
                RepeatedOutliers = new AssetPairExtPriceSettingsStorageModel.RepeatedOutliersParamsStorageModel
                {
                    MaxSequenceLength = 10,
                    MaxSequenceAge = TimeSpan.FromMinutes(5),
                    MaxAvg = 1,
                    MaxAvgAge = TimeSpan.FromMinutes(5)
                },
                Steps = Enum.GetValues(typeof(OrderbookGeneratorStepDomainEnum))
                    .Cast<OrderbookGeneratorStepDomainEnum>()
                    .ToImmutableSortedDictionary(e => e, e => true)
                    .SetItem(OrderbookGeneratorStepDomainEnum.GetArbitrageFreeSpread, false),
                Exchanges = ImmutableSortedDictionary<string, ExchangeExtPriceSettingsStorageModel>.Empty
                    .Add("bitmex", GetDefaultExtPriceExchangeSettings())
                    .Add("bitfinex", GetDefaultExtPriceExchangeSettings())
                    .Add("Poloniex", GetDefaultExtPriceExchangeSettings())
                    .Add("Kraken", GetDefaultExtPriceExchangeSettings())
            };
        }

        private static ExchangeExtPriceSettingsStorageModel GetDefaultExtPriceExchangeSettings()
        {
            return new ExchangeExtPriceSettingsStorageModel
            {
                OrderbookOutdatingThreshold = TimeSpan.FromSeconds(30),
                Disabled = new ExchangeExtPriceSettingsStorageModel.DisabledSettings {Reason = ""},
                Hedging = new ExchangeExtPriceSettingsStorageModel.HedgingSettings(),
                OrderGeneration = new ExchangeExtPriceSettingsStorageModel.OrderGenerationSettings
                {
                    VolumeMultiplier = 1,
                    OrderRenewalDelay = TimeSpan.FromSeconds(10)
                }
            };
        }

        private static CrossRateCalcInfoStorageModel GetDefaultCrossRateCalcInfo(string assetPairId)
        {
            return new CrossRateCalcInfoStorageModel
            {
                ResultingPairId = assetPairId,
                Source1 = new CrossRateCalcInfoStorageModel.CrossRateSourceAssetPair
                {
                    Id = string.Empty,
                },
                Source2 = new CrossRateCalcInfoStorageModel.CrossRateSourceAssetPair
                {
                    Id = string.Empty,
                },
            };
        }

        private static AggregateOrderbookSettingsStorageModel GetDefaultAggregateOrderbookSettings()
        {
            return new AggregateOrderbookSettingsStorageModel
            {
                AsIsLevelsCount = int.MaxValue,
                CumulativeVolumeLevels = ImmutableSortedSet<decimal>.Empty,
                RandomFraction = 0.05m
            };
        }
    }
}