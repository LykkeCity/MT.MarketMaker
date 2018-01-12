using System;
using Common;
using Common.Log;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class PriceRoundingService : IPriceRoundingService
    {
        private readonly IAssetPairsInfoService _assetPairsInfoService;
        private readonly ILog _log;

        public PriceRoundingService(IAssetPairsInfoService assetPairsInfoService, ILog log)
        {
            _assetPairsInfoService = assetPairsInfoService;
            _log = log;
        }

        public Func<decimal, decimal> GetRoundFunc(string assetPairId)
        {
            var accuracy = _assetPairsInfoService.Get(assetPairId)?.Accuracy;
            if (accuracy == null)
            {
                _log.WriteWarningAsync(nameof(PriceRoundingService), new {AssetPairId = assetPairId}.ToJson(),
                    $"Accuracy for assetPair {assetPairId} was not found");
                return p => p;
            }
                    
            return p => Math.Round(p, accuracy.Value);
        }
    }
}