using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Services.Common
{
    internal interface ISettingsRootService
    {
        Task Set(SettingsRoot settings);
        SettingsRoot Get();
        [CanBeNull] AssetPairSettings Get(string assetPairId);
        Task Set(string assetPairId, Func<AssetPairSettings, AssetPairSettings> changeFunc);
    }
}