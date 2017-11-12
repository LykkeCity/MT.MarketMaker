using System.Collections.Immutable;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories.Entities;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Settings;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class CrossRatesSettingsRepository
        : AbstractRepository<CrossRatesSettingsEntity, CrossRatesSettings>, ICrossRatesSettingsRepository
    {
        public CrossRatesSettingsRepository(IReloadingManager<MarginTradingMarketMakerSettings> settings, ILog log) :
            base(AzureTableStorage<CrossRatesSettingsEntity>.Create(
                settings.Nested(s => s.Db.ConnectionString),
                "MarketMakerCrossRatesSettings", log))
        {
        }

        protected override CrossRatesSettings Convert(CrossRatesSettingsEntity entity)
        {
            return new CrossRatesSettings(
                entity.BaseAssetId,
                JsonConvert.DeserializeObject<ImmutableArray<string>>(entity.OtherAssetsIds ?? "[]"));
        }

        protected override CrossRatesSettingsEntity Convert(CrossRatesSettings dto)
        {
            return new CrossRatesSettingsEntity
            {
                BaseAssetId = dto.BaseAssetId,
                OtherAssetsIds = JsonConvert.SerializeObject(dto.OtherAssetsIds),
            };
        }
    }
}