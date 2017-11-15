using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.AzureRepositories.Entities;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface IExchangeExtPriceSettingsRepository : IAbstractRepository<ExchangeExtPriceSettingsEntity>
    {
        Task<IEnumerable<ExchangeExtPriceSettingsEntity>> GetAsync(string partitionKey);
        Task DeleteAsync(IEnumerable<ExchangeExtPriceSettingsEntity> entities);
    }
}