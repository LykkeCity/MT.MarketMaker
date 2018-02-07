using System;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    internal class PriceRoundingService : IPriceRoundingService
    {
        private readonly IAssetPairsInfoService _assetPairsInfoService;

        public PriceRoundingService(IAssetPairsInfoService assetPairsInfoService)
        {
            _assetPairsInfoService = assetPairsInfoService;
        }

        public Func<decimal, decimal> GetRoundFunc(string assetPairId)
        {
            var accuracy = _assetPairsInfoService.Get(assetPairId)?.Accuracy ?? 8;
            return p => Math.Round(p, accuracy);
        }
    }
}