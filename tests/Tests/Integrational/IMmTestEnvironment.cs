using System;
using System.Collections.Generic;
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;

namespace Tests.Integrational
{
    internal interface IMmTestEnvironment : ITestEnvironment
    {
        DateTime UtcNow { get; set; }
        StubRabbitMqService StubRabbitMqService { get; }
        List<AssetPairContract> AssetPairs { get; set; }
        SettingsRootStorageModel SettingsRoot { get; set; }
        InMemoryTableStorageFactory TableStorageFactory { get; }
        InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; }
    }
}