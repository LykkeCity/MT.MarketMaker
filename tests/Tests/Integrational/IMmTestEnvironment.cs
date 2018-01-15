using System;
using System.Collections.Generic;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Models.Settings;

namespace Tests.Integrational
{
    internal interface IMmTestEnvironment : ITestEnvironment
    {
        DateTime UtcNow { get; set; }
        StubRabbitMqService StubRabbitMqService { get; }
        IList<AssetPairResponseModel> AssetPairs { get; set; }
        SettingsRoot SettingsRoot { get; set; }
        InMemoryTableStorageFactory TableStorageFactory { get; }
    }
}