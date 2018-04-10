using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.AssetPairSettings;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class AssetPairsInfoService : TimerPeriod, IAssetPairsInfoService, ICustomStartup
    {
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private readonly IConvertService _convertService;
        private IReadOnlyDictionary<string, AssetPairInfo> _assetPairs;
        private readonly ManualResetEventSlim _assetPairsInitializedEvent = new ManualResetEventSlim();
        private readonly IReloadingManager<MarginTradingMarketMakerSettings> _settings;

        public AssetPairsInfoService(IMtDataReaderClient mtDataReaderClient, IConvertService convertService, ILog log,
            IReloadingManager<MarginTradingMarketMakerSettings> settings)
            : base(nameof(AssetPairsInfoService), (int) TimeSpan.FromMinutes(3).TotalMilliseconds + 1, log)
        {
            _mtDataReaderClient = mtDataReaderClient;
            _convertService = convertService;
            _settings = settings;
        }

        public IReadOnlyDictionary<string, AssetPairInfo> Get()
        {
            return _assetPairs;
        }

        public AssetPairInfo Get(string assetPairId)
        {
            return _assetPairs.GetValueOrDefault(assetPairId);
        }

        public override async Task Execute()
        {
            _assetPairs = (await _mtDataReaderClient.AssetPairsRead.List(_settings.CurrentValue.LegalEntity,
                    MatchingEngineModeContract.MarketMaker))
                .ToDictionary(s => s.Id, Convert);
            _assetPairsInitializedEvent.Set();
        }

        private AssetPairInfo Convert(AssetPairContract pair)
        {
            return _convertService.Convert<AssetPairContract, AssetPairInfo>(pair);
        }

        public void Initialize()
        {
            Start();
            _assetPairsInitializedEvent.Wait();
        }
    }
}