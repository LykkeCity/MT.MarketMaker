﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Api;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    public class TestingHelperService : ITestingHelperService
    {
        private readonly ReadWriteLockedDictionary<(string AssetPairId, string Exchange), ImmutableList<TestSettingModel>> _settings =
            new ReadWriteLockedDictionary<(string, string), ImmutableList<TestSettingModel>>();

        public IReadOnlyDictionary<(string AssetPairId, string Exchange), ImmutableList<TestSettingModel>> GetAll()
        {
            return _settings;
        }

        public ImmutableList<TestSettingModel> Get(string assetPairId, string exchange)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _settings.GetOrDefault((assetPairId, exchange), k => ImmutableList<TestSettingModel>.Empty);
        }

        public void Delete(string assetPairId, string exchange)
        {
            _settings.Remove((assetPairId, exchange));
        }

        public void DeleteAll()
        {
            _settings.Clear();
        }

        public void Add(ImmutableList<TestSettingModel> settings)
        {
            foreach (var s in settings)
            {
                _settings.AddOrUpdate((s.AssetPairId, s.Exchange), k => ImmutableList.Create(s), (k, old) => old.Add(s));
            }
        }

        public ExternalOrderbook ModifyOrderbookIfNeeded(ExternalOrderbook orderbook)
        {
            if (!_settings.TryGetValue((orderbook.AssetPairId, orderbook.ExchangeName), out var settings))
            {
                return orderbook;
            }

            var now = orderbook.LastUpdatedTime;
            var bids = orderbook.Bids;
            var asks = orderbook.Asks;
            foreach (var s in settings)
            {
                if (IsActive(s.Disable, now))
                {
                    return null; // skip orderbook
                }

                if (IsActive(s.BidModification, now))
                {
                    bids = Shift(bids, s.BidModification.Amount);
                }

                if (IsActive(s.AskModification, now))
                {
                    asks = Shift(asks, s.AskModification.Amount);
                }
            }

            return new ExternalOrderbook(orderbook.AssetPairId, orderbook.ExchangeName, now, bids, asks);
        }

        [ContractAnnotation("r:null => false")]
        private static bool IsActive([CanBeNull] TestSettingModel.Range r, DateTime now)
        {
            return r != null && (r.Start <= now || r.Start == null) && (r.End >= now || r.End == null);
        }

        private static ImmutableArray<OrderbookPosition> Shift(ImmutableArray<OrderbookPosition> positions, decimal amount)
        {
            return positions.Select(p => new OrderbookPosition(p.Price + amount, p.Volume)).ToImmutableArray();
        }
    }
}
