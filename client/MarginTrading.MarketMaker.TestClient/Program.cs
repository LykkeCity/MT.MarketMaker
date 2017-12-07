using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Client;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.MarketMaker.TestClient
{
    internal static class Program
    {
        private const string AssetPairId = "BTCUSD";
        private const string TestAssetPairId = "BTCUSD_test";
        private const string TestExchangeName = "TestExchange";
        private static int _counter;

        static async Task Main(string[] args)
        {
            try
            {
                await Run();
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.Content);
                throw;
            }
        }

        private static async Task Run()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            services.RegisterMtMarketMakerClient("http://localhost:5007", "TestClient");
            builder.Populate(services);
            var container = builder.Build();
            var client = container.Resolve<IMtMarketMakerClient>();
            
            await client.AssetPairs.List().Dump();
            await client.AssetPairs.Get(AssetPairId).Dump();
            await client.AssetPairs.Delete(TestAssetPairId).Dump();
            await client.AssetPairs.Add(TestAssetPairId, AssetPairQuotesSourceTypeEnum.External).Dump();
            await client.AssetPairs.Get(TestAssetPairId).Dump();

            await client.ExtPriceSettings.List().Dump();
            await client.ExtPriceSettings.Get(AssetPairId).Dump();
            var extPriceSettings = await client.ExtPriceSettings.Get(TestAssetPairId).Dump();
            extPriceSettings.OutlierThreshold = 0.9m;
            await client.ExtPriceSettings.Update(extPriceSettings).Dump();
            await client.ExtPriceSettings.Get(TestAssetPairId).Dump();
            await client.ExtPriceSettings.GetHedgingPreferences().Dump();

            await client.ExtPriceExchanges.List().Dump();
            await client.ExtPriceExchanges.ListByAssetPair(AssetPairId).Dump();
            await client.ExtPriceExchanges.Get(AssetPairId, "Bitmex").Dump();
            await client.ExtPriceExchanges.Add(TestAssetPairId, TestExchangeName).Dump();
            var testExchange = await client.ExtPriceExchanges.Get(TestAssetPairId, TestExchangeName).Dump();
            testExchange.OrderbookOutdatingThreshold *= 2;
            await client.ExtPriceExchanges.Update(testExchange).Dump();
            await client.ExtPriceExchanges.Delete(TestAssetPairId, TestExchangeName).Dump();
            await client.ExtPriceExchanges.Get(TestAssetPairId, TestExchangeName).Dump();

            await client.ExtPriceStatus.List().Dump();
            await client.ExtPriceStatus.Get(AssetPairId).Dump();
            await client.ExtPriceStatus.GetLogs().Dump();
            await client.ExtPriceStatus.GetLogsFiltered(AssetPairId).Dump();

            var root = await client.SettingsRoot.Get().Dump();
            await client.SettingsRoot.Set(root).Dump();
            
            await client.AssetPairs.Delete(TestAssetPairId).Dump();
            await client.AssetPairs.Get(TestAssetPairId).Dump();
        }

        public static T Dump<T>(this T o)
        {
            Console.WriteLine("{0}. {1}", ++_counter, JsonConvert.SerializeObject(o));
            return o;
        }
        
        public static async Task<T> Dump<T>(this Task<T> t)
        {
            var obj = await t;
            obj.Dump();
            return obj;
        }
        
        public static async Task Dump(this Task o)
        {
            await o;
            "ok".Dump();
        }
    }
}