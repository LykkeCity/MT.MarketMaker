using System.Collections.Immutable;
using AzureStorage;
using MarginTrading.MarketMaker.AzureRepositories.Entities;
using MarginTrading.MarketMaker.Models;
using Newtonsoft.Json;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class CrossRatesSettingsRepository : AbstractRepository<CrossRatesSettingsEntity, CrossRatesSettings>, ICrossRatesSettingsRepository
    {
        public CrossRatesSettingsRepository(INoSQLTableStorage<CrossRatesSettingsEntity> tableStorage) : base(
            tableStorage)
        {
        }

        protected override CrossRatesSettings Convert(CrossRatesSettingsEntity entity)
        {
            return new CrossRatesSettings(
                JsonConvert.DeserializeObject<ImmutableArray<string>>(entity.BaseAssetsIds ?? "[]"),
                JsonConvert.DeserializeObject<ImmutableArray<string>>(entity.OtherAssetsIds ?? "[]"));
        }

        protected override CrossRatesSettingsEntity Convert(CrossRatesSettings dto)
        {
            return new CrossRatesSettingsEntity
            {
                BaseAssetsIds = JsonConvert.SerializeObject(dto.BaseAssetsIds),
                OtherAssetsIds = JsonConvert.SerializeObject(dto.OtherAssetsIds),
            };
        }
    }
}