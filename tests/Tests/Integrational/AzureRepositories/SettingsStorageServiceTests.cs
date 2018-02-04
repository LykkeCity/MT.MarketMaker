using System.Linq;
using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.AzureRepositories.Implementation;
using MoreLinq;
using NUnit.Framework;

namespace Tests.Integrational.AzureRepositories
{
    public class SettingsStorageServiceTests
    {
        private readonly MmIntegrationalTestSuit _testSuit = new MmIntegrationalTestSuit();
        
        [Test]
        public void IfModelVersionDiffers_ShouldMigrate()
        {
            //arrange
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var settingsStorageService = container.Resolve<ISettingsStorageService>();
            var settingsRootModel = env.SettingsRoot;
            settingsRootModel.Version = 1;
            settingsRootModel.AssetPairs.ForEach(p => p.Value.AggregateOrderbookSettings = null);

            //act
            var root = settingsStorageService.Read();
            settingsStorageService.Write(root);
            
            //assert
            settingsRootModel = env.SettingsRoot;
            settingsRootModel.Version.Should().Be(SettingsStorageService.CurrentStorageModelVersion);
            settingsRootModel.AssetPairs.Should().Match(d => d.Values.All(v => v.AggregateOrderbookSettings != null));
        }
        
        [Test]
        public void Always_ShouldCorrectlyReadWrite()
        {
            //arrange
            var env = _testSuit.Build();
            var container = env.CreateContainer();
            var settingsStorageService = container.Resolve<ISettingsStorageService>();
            var origSettingsRootModel = env.SettingsRoot;

            //act
            var root = settingsStorageService.Read();
            settingsStorageService.Write(root);
            
            //assert
            origSettingsRootModel.Should().NotBe(env.SettingsRoot);
            origSettingsRootModel.Should().BeEquivalentTo(env.SettingsRoot);
        }
    }
}