using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    public class AssetPairsInfoService : IAssetPairsInfoService
    {
        private readonly ISystem _system;
        private readonly IAssetsService _assetsService;
        private readonly IConvertService _convertService;
        private readonly ICachedCalculation<IReadOnlyDictionary<string, AssetPairInfo>> _assetPairs;

        public AssetPairsInfoService(ISystem system, IAssetsService assetsService, IConvertService convertService)
        {
            _system = system;
            _assetsService = assetsService;
            _convertService = convertService;
            _assetPairs = GetAssetPairsCache();
        }

        public IReadOnlyDictionary<string, AssetPairInfo> Get()
        {
            return _assetPairs.Get();
        }
        
        public AssetPairInfo Get(string assetPairId)
        {
            return _assetPairs.Get().GetValueOrDefault(assetPairId);
        }

        private ICachedCalculation<Dictionary<string, AssetPairInfo>> GetAssetPairsCache()
        {
            return Calculate.Cached(() => _system.UtcNow,
                (prev, now) => now.Subtract(prev) < TimeSpan.FromMinutes(5),
                now => _assetsService.AssetPairGetAll().ToDictionary(p => p.Id,
                    p => _convertService.Convert<AssetPair, AssetPairInfo>(p,
                        o => o.ConfigureMap(MemberList.Destination))));
        }
    }
}