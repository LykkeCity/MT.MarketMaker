using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.AzureRepositories;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Services.CrossRates.Models;
using MoreLinq;

namespace MarginTrading.MarketMaker.Services.CrossRates.Implementation
{
    public class CrossRateCalcInfosService : ICrossRateCalcInfosService, IStartable
    {
        private ImmutableArray<CrossRateCalcInfo> _cache;
        private readonly ICachedCalculation<ILookup<string, CrossRateCalcInfo>> _dependentAssetPairs;

        private readonly ICrossRateCalcInfosRepository _repository;

        public CrossRateCalcInfosService(ICrossRateCalcInfosRepository repository)
        {
            _repository = repository;
            _dependentAssetPairs = DependentAssetPairsCache();
        }

        public void Set([NotNull] IEnumerable<CrossRateCalcInfo> models)
        {
            var newCache = models.RequiredNotNullElems(nameof(models)).ToImmutableArray();
            WriteRepository(newCache);
            _cache = newCache;
        }

        public IReadOnlyList<CrossRateCalcInfo> Get()
        {
            return _cache;
        }

        [ItemNotNull]
        public IEnumerable<CrossRateCalcInfo> GetDependentAssetPairs([NotNull] string assetPairId)
        {
            if (assetPairId == null) throw new ArgumentNullException(nameof(assetPairId));
            return _dependentAssetPairs.Get()[assetPairId].RequiredNotNullElems("result");
        }

        private ICachedCalculation<ILookup<string, CrossRateCalcInfo>> DependentAssetPairsCache()
        {
            return Calculate.Cached(Get, ReferenceEquals,
                src => src.SelectMany(i => new[] { (i.Source1.Id, i), (i.Source2.Id, i) })
                    .ToLookup());
        }

        private void WriteRepository(IEnumerable<CrossRateCalcInfo> models)
        {
            _repository.InsertOrReplaceAsync(models).GetAwaiter().GetResult();
        }

        private ImmutableArray<CrossRateCalcInfo> ReadRepository()
        {
            return _repository.GetAllAsync().GetAwaiter().GetResult().ToImmutableArray();
        }

        public void Start()
        {
            _cache = ReadRepository();
        }
    }
}