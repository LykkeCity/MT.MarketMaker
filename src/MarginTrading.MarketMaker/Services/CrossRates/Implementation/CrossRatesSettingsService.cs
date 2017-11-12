using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRatesSettingsService : ICrossRatesSettingsService
    {
        [CanBeNull]
        private IReadOnlyList<CrossRatesSettings> _cache;

        private readonly object _writeLock = new object();

        private readonly ICrossRatesSettingsRepository _repository;

        public CrossRatesSettingsService(ICrossRatesSettingsRepository repository)
        {
            _repository = repository;
        }

        public void Set([NotNull] IReadOnlyList<CrossRatesSettings> models)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));
            lock (_writeLock)
            {
                WriteRepository(models);
                _cache = models;
            }
        }

        public IReadOnlyList<CrossRatesSettings> Get()
        {
            if (_cache == null)
            {
                lock (_writeLock)
                {
                    if (_cache == null)
                    {
                        _cache = ReadRepository();
                    }
                }
            }

            return _cache.RequiredNotNull("result");
        }

        private void WriteRepository(IEnumerable<CrossRatesSettings> models)
        {
            _repository.InsertOrReplaceAsync(models).GetAwaiter().GetResult();
        }

        private IReadOnlyList<CrossRatesSettings> ReadRepository()
        {
            return _repository.GetAllAsync().GetAwaiter().GetResult();
        }
    }
}