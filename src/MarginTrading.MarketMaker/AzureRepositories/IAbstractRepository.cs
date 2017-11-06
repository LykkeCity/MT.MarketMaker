using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    public interface IAbstractRepository<TDto>
    {
        Task InsertOrReplaceAsync(TDto entity);
        [ItemCanBeNull]
        Task<TDto> GetAsync(TDto dto);
        Task<IReadOnlyList<TDto>> GetAllAsync();
        Task DeleteIfExistAsync(TDto dto);
    }
}