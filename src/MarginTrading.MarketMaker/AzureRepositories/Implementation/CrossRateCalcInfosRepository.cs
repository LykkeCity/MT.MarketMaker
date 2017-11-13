using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories.Entities;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class CrossRateCalcInfosRepository
        : AbstractRepository<CrossRateCalcInfoEntity, CrossRateCalcInfo>, ICrossRateCalcInfosRepository
    {
        public CrossRateCalcInfosRepository(IReloadingManager<MarginTradingMarketMakerSettings> settings, ILog log) :
            base(AzureTableStorage<CrossRateCalcInfoEntity>.Create(
                settings.Nested(s => s.Db.ConnectionString),
                "MarketMakerCrossRateCalcInfos", log))
        {
        }

        protected override CrossRateCalcInfo Convert(CrossRateCalcInfoEntity entity)
        {
            return new CrossRateCalcInfo(entity.ResultingPairId,
                new CrossRateSourceAssetPair(entity.SourcePairId1, entity.IsTransitoryAssetQuoting1),
                new CrossRateSourceAssetPair(entity.SourcePairId2, entity.IsTransitoryAssetQuoting2));
        }

        protected override CrossRateCalcInfoEntity Convert(CrossRateCalcInfo dto)
        {
            return new CrossRateCalcInfoEntity
            {
                ResultingPairId = dto.ResultingPairId,
                SourcePairId1 = dto.Source1.Id,
                IsTransitoryAssetQuoting1 = dto.Source1.IsTransitoryAssetQuoting,
                SourcePairId2 = dto.Source2.Id,
                IsTransitoryAssetQuoting2 = dto.Source2.IsTransitoryAssetQuoting
            };
        }
    }
}
