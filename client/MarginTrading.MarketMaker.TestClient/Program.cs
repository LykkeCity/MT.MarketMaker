using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts;
using MarginTrading.MarketMaker.Contracts.Client;
using MarginTrading.MarketMaker.Contracts.Enums;
using MarginTrading.MarketMaker.Contracts.Models;
using Microsoft.AspNetCore.Http;
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
                var str = e.Content;
                if (str.StartsWith('"'))
                {
                    str = TryDeserializeToString(str);
                }
                
                Console.WriteLine(str);
                Console.WriteLine(e.ToAsyncString());
            }
        }

        private static string TryDeserializeToString(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(str);
            }
            catch
            {
                return str;
            }
        }

        private static async Task Run()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.RegisterMtMarketMakerClient("http://localhost:5007", "TestClient");
            builder.Populate(services);
            var container = builder.Build();
            var client = container.Resolve<IMtMarketMakerClient>();
            
            await client.AssetPairs.List().Dump();
            await client.AssetPairs.Get(AssetPairId).Dump();
            await TryDeleteOld(client);
            await client.AssetPairs.Add(TestAssetPairId, AssetPairQuotesSourceTypeEnum.Manual).Dump();
            await client.AssetPairs.Get(TestAssetPairId).Dump();
            await client.AssetPairs.Update(new AssetPairInputModel{AssetPairId = TestAssetPairId, SourceType = AssetPairQuotesSourceTypeEnum.Disabled}).Dump();
            var updatedPair = await client.AssetPairs.Get(TestAssetPairId).Dump();
            if (updatedPair.SourceType != "Disabled")
            {
                throw new Exception("SourceType not updated");
            }

            await client.ExtPriceSettings.List().Dump();
            await client.ExtPriceSettings.Get(AssetPairId).Dump();
            var extPriceSettings = await client.ExtPriceSettings.Get(TestAssetPairId).Dump();
            extPriceSettings.OutlierThreshold = 0.9m;
            await client.ExtPriceSettings.Update(extPriceSettings).Dump();
            var extPriceSettings2 = await client.ExtPriceSettings.Get(TestAssetPairId).Dump();
            if (extPriceSettings2.OutlierThreshold != extPriceSettings.OutlierThreshold)
            {
                throw new Exception("extPriceSettings.OutlierThreshold not changed");
            }
            
            await client.ExtPriceSettings.GetHedgingPreferences().Dump();

            await client.ExtPriceExchanges.List().Dump();
            await client.ExtPriceExchanges.ListByAssetPair(AssetPairId).Dump();
            await client.ExtPriceExchanges.Get(AssetPairId, "Bitmex").Dump();
            await client.ExtPriceExchanges.Add(TestAssetPairId, TestExchangeName).Dump();
            var testExchange = await client.ExtPriceExchanges.Get(TestAssetPairId, TestExchangeName).Dump();
            testExchange.OrderbookOutdatingThreshold *= 2;
            await client.ExtPriceExchanges.Update(testExchange).Dump();
            var testExchange2 = await client.ExtPriceExchanges.Get(TestAssetPairId, TestExchangeName).Dump();
            if (testExchange2.OrderbookOutdatingThreshold != testExchange.OrderbookOutdatingThreshold)
            {
                throw new Exception("OrderbookOutdatingThreshold not changed");
            }
            
            await client.ExtPriceExchanges.Delete(TestAssetPairId, TestExchangeName).Dump();
            var deletedExchange = await client.ExtPriceExchanges.Get(TestAssetPairId, TestExchangeName).Dump();
            if (deletedExchange != null) throw new Exception("Exchange not deleted");
            await client.ExtPriceStatus.List().Dump();
            await client.ExtPriceStatus.Get(AssetPairId).Dump();
            await client.ExtPriceStatus.GetLogs().Dump();
            await client.ExtPriceStatus.GetLogsFiltered(AssetPairId).Dump();

            var root = await client.SettingsRoot.Get().Dump();
            await client.SettingsRoot.Set(root).Dump();
            
            await client.AssetPairs.Delete(TestAssetPairId).Dump();
            var deletedAssetPair = await client.AssetPairs.Get(TestAssetPairId).Dump();
            if (deletedAssetPair != null) throw new Exception("deletedAssetPair != null");
            await client.CrossRateCalcInfos.List().Dump();            
            await client.CrossRateCalcInfos.Get("BTCUSD").Dump();
            var btcUsdCrossRate = await client.CrossRateCalcInfos.Get("BTCUSD").Dump();
            if (btcUsdCrossRate != null) throw new Exception("btcUsdCrossRate != null");
            
            var testCrossRate = await client.CrossRateCalcInfos.Get("BTCGBP").Dump();
            testCrossRate.Source1.IsTransitoryAssetQuoting = !testCrossRate.Source1.IsTransitoryAssetQuoting; 
            await client.CrossRateCalcInfos.Update(testCrossRate).Dump();            
            var testCrossRate2 = await client.CrossRateCalcInfos.Get("BTCGBP").Dump();
            if (testCrossRate2.Source1.IsTransitoryAssetQuoting != testCrossRate.Source1.IsTransitoryAssetQuoting)
            {
                throw new Exception("IsTransitoryAssetQuoting not changed");
            }
                
            testCrossRate.Source1.IsTransitoryAssetQuoting = !testCrossRate.Source1.IsTransitoryAssetQuoting;
            await client.CrossRateCalcInfos.Update(testCrossRate).Dump();         
            Console.WriteLine("Successfuly finished");
        }

        private static async Task TryDeleteOld(IMtMarketMakerClient client)
        {
            try
            {
                await client.AssetPairs.Delete(TestAssetPairId).Dump();
            }
            catch
            {
            }
        }

        [CanBeNull]
        public static T Dump<T>(this T o)
        {
            var str = o is string s ? s : JsonConvert.SerializeObject(o);
            Console.WriteLine("{0}. {1}", ++_counter, str);
            return o;
        }
        
        [ItemCanBeNull]
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