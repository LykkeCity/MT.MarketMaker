using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface ITestingHelperService
    {
        IReadOnlyDictionary<(string AssetPairId, string Exchange), ImmutableList<TestSettingModel>> GetAll();
        ImmutableList<TestSettingModel> Get(string assetPairId, string exchange);
        void Delete(string assetPairId, string exchange);
        void DeleteAll();
        void Add(ImmutableList<TestSettingModel> settings);
        [CanBeNull]
        ExternalOrderbook ModifyOrderbookIfNeeded(ExternalOrderbook orderbook);
    }
}