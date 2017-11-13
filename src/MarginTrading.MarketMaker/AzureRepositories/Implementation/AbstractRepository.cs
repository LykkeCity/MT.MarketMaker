using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
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

        public Task InsertOrReplaceAsync(IEnumerable<TEntity> entities)
        {
            return TableStorage.InsertOrReplaceAsync(entities);
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

        public Task InsertOrReplaceAsync([NotNull] TDto entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return TableStorage.InsertOrReplaceAsync(Convert(entity));
        }

        public Task InsertOrReplaceAsync(IEnumerable<TDto> entities)
        {
            return TableStorage.InsertOrReplaceAsync(entities.RequiredNotNullElems("entities").Select(Convert));
        }

        public async Task<TDto> GetAsync(TDto dto)
        {
            var entity = Convert(dto);
            return Convert(await TableStorage.GetDataAsync(entity.PartitionKey, entity.RowKey));
        }

        public async Task<IReadOnlyList<TDto>> GetAllAsync()
        {
            return (await TableStorage.GetDataAsync()).Select(Convert).RequiredNotNullElems("result").ToList();
        }

        public Task DeleteIfExistAsync([NotNull] TDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var entity = Convert(dto);
            return TableStorage.DeleteIfExistAsync(entity.PartitionKey, entity.RowKey);
        }

        protected abstract TDto Convert(TEntity entity);

        protected abstract TEntity Convert(TDto dto);
    }
}
