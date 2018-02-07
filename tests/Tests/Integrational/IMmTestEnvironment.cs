using System;
using System.Collections.Generic;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.AzureRepositories.StorageModels;
using MarginTrading.MarketMaker.Models.Settings;

namespace Tests.Integrational
{
    internal interface IMmTestEnvironment : ITestEnvironment
    {
        DateTime UtcNow { get; set; }
        StubRabbitMqService StubRabbitMqService { get; }
        IList<AssetPair> AssetPairs { get; set; }
        SettingsRootStorageModel SettingsRoot { get; set; }
        InMemoryTableStorageFactory TableStorageFactory { get; }
        InMemoryBlobStorageSingleObjectFactory BlobStorageFactory { get; }
    }
}