using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    internal class CrossRatesSettingsService : ICrossRatesSettingsService
    {
        [CanBeNull]
        private CrossRatesSettings _cache;

        private readonly object _writeLock = new object();

        private readonly ICrossRatesSettingsRepository _repository;

        public CrossRatesSettingsService(ICrossRatesSettingsRepository repository)
        {
            _repository = repository;
        }

        public void Set(CrossRatesSettings model)
        {
            lock (_writeLock)
            {
                WriteRepository(model);
                _cache = model;
            }
        }

        public CrossRatesSettings Get()
        {
            if (_cache == null)
            {
                lock (_writeLock)
                {
                    if (_cache == null)
                    {
                        _cache = ReadRepository()
                                 ?? new CrossRatesSettings(ImmutableArray<string>.Empty, ImmutableArray<string>.Empty);
                    }
                }
            }

            return _cache;
        }

        private void WriteRepository(CrossRatesSettings model)
        {
            _repository.InsertOrReplaceAsync(model).GetAwaiter().GetResult();
        }

        [CanBeNull]
        private CrossRatesSettings ReadRepository()
        {
            return _repository.GetAllAsync().GetAwaiter().GetResult().FirstOrDefault();
        }
    }
}