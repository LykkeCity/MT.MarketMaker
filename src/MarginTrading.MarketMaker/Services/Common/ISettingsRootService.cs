﻿using System;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common
{
    /// <remarks>
    /// No async here because of a lock inside
    /// </remarks>
    public interface ISettingsRootService
    {
        void Set(SettingsRoot settings);
        SettingsRoot Get();
        [CanBeNull] AssetPairSettings Get(string assetPairId);
        void Update(string assetPairId, Func<AssetPairSettings, AssetPairSettings> changeFunc);
        void Delete(string assetPairId);
        void Add(string assetPairId, AssetPairSettings settings);
    }
}