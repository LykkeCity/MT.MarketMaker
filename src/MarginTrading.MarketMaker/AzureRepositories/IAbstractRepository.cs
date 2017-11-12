using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    public interface IAbstractRepository<TDto>
    {
        Task InsertOrReplaceAsync(TDto entity);
        Task InsertOrReplaceAsync(IEnumerable<TDto> entities);
        [ItemCanBeNull]
        Task<TDto> GetAsync(TDto dto);
        Task<IReadOnlyList<TDto>> GetAllAsync();
        Task DeleteIfExistAsync(TDto dto);
    }
}