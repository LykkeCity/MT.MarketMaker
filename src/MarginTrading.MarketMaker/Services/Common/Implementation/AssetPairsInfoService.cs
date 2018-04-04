using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    public class AssetPairsInfoService : TimerPeriod, IAssetPairsInfoService, ICustomStartup
    {
        private readonly IAssetsService _assetsService;
        private readonly IConvertService _convertService;
        private IReadOnlyDictionary<string, AssetPairInfo> _assetPairs;
        private readonly ManualResetEventSlim _assetPairsInitializedEvent = new ManualResetEventSlim();

        public AssetPairsInfoService(IAssetsService assetsService, IConvertService convertService, ILog log) 
            : base(nameof(AssetPairsInfoService), (int) TimeSpan.FromMinutes(2).TotalMilliseconds, log)
        {
            _assetsService = assetsService;
            _convertService = convertService;
        }

        public IReadOnlyDictionary<string, AssetPairInfo> Get()
        {
            return _assetPairs;
        }
        
        public AssetPairInfo Get(string assetPairId)
        {
            return _assetPairs.GetValueOrDefault(assetPairId);
        }

        public override Task Execute()
        {
            _assetPairs =  _assetsService.AssetPairGetAll().ToDictionary(p => p.Id,
                p => _convertService.Convert<AssetPair, AssetPairInfo>(p,
                    o => o.ConfigureMap(MemberList.Destination)));
            _assetPairsInitializedEvent.Set();
            return Task.CompletedTask;
        }

        public void Initialize()
        {
            Start();
            _assetPairsInitializedEvent.Wait();
        }
    }
}