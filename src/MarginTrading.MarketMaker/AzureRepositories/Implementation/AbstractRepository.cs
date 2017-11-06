using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    [Obsolete("Use AbstractRepository`2")]
    internal abstract class AbstractRepository<TEntity> : IAbstractRepository<TEntity> where TEntity : ITableEntity, new()
    {
        protected readonly INoSQLTableStorage<TEntity> TableStorage;

        protected AbstractRepository(INoSQLTableStorage<TEntity> tableStorage)
        {
            TableStorage = tableStorage;
        }

        public Task InsertOrReplaceAsync(TEntity entity)
        {
            return TableStorage.InsertOrReplaceAsync(entity);
        }

        public Task<TEntity> GetAsync(TEntity entity)
        {
            return TableStorage.GetDataAsync(entity.PartitionKey, entity.RowKey);
        }

        public async Task<IReadOnlyList<TEntity>> GetAllAsync()
        {
            return (await TableStorage.GetDataAsync()).ToList();
        }

        public Task DeleteIfExistAsync(TEntity entity)
        {
            return TableStorage.DeleteIfExistAsync(entity.PartitionKey, entity.RowKey);
        }
    }

    internal abstract class AbstractRepository<TEntity, TDto> : IAbstractRepository<TDto>
        where TEntity : ITableEntity, new()
    {
        protected readonly INoSQLTableStorage<TEntity> TableStorage;

        protected AbstractRepository(INoSQLTableStorage<TEntity> tableStorage)
        {
            TableStorage = tableStorage;
        }

        public Task InsertOrReplaceAsync(TDto entity)
        {
            return TableStorage.InsertOrReplaceAsync(Convert(entity));
        }

        public async Task<TDto> GetAsync(TDto dto)
        {
            var entity = Convert(dto);
            return Convert(await TableStorage.GetDataAsync(entity.PartitionKey, entity.RowKey));
        }

        public async Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return (await TableStorage.GetDataAsync()).Select(Convert).ToList();
        }

        public Task DeleteIfExistAsync(TDto dto)
        {
            var entity = Convert(dto);
            return TableStorage.DeleteIfExistAsync(entity.PartitionKey, entity.RowKey);
        }

        protected abstract TDto Convert(TEntity entity);

        protected abstract TEntity Convert(TDto dto);
    }
}
