using System;
using MarginTrading.MarketMaker.Infrastructure.Implementation;
using MarginTrading.MarketMaker.Models.Settings;
using MarginTrading.MarketMaker.Services.CrossRates.Models;

namespace MarginTrading.MarketMaker.Services.Common.Implementation
{
    public class SettingsValidationService : ISettingsValidationService
    {
        public void Validate(SettingsRoot root)
        {
            root.AssetPairs.RequiredNotNull("root.AssetPairs");
            foreach (var (assetPairId, pairSettings) in root.AssetPairs)
                new AssetPairValidator(assetPairId, pairSettings).Validate();
        }

        private struct AssetPairValidator
        {
            private readonly string _assetPairId;
            private readonly AssetPairSettings _pairSettings;

            public AssetPairValidator(string assetPairId, AssetPairSettings pairSettings)
            {
                _assetPairId = assetPairId;
                _pairSettings = pairSettings;
            }

            public void Validate()
            {
                _pairSettings.RequiredNotNull("_pairSettings for pair " + _assetPairId);
                _pairSettings.QuotesSourceType.RequiredEnum(
                    "_pairSettings.QuotesSourceType for pair " + _assetPairId);
                Validate(_pairSettings.CrossRateCalcInfo);
                Validate(_pairSettings.ExtPriceSettings);
            }

            private void Validate(AssetPairExtPriceSettings extPriceSettings)
            {
                extPriceSettings.PresetDefaultExchange.RequiredInSet(extPriceSettings.Exchanges.Keys,
                    "extPriceSettings.Exchanges.Keys for pair " + _assetPairId);
                extPriceSettings.OutlierThreshold.RequiredBetween(0.00001m, 0.5m,
                    "extPriceSettings.OutlierThreshold for " + _assetPairId);
                extPriceSettings.MinOrderbooksSendingPeriod.RequiredBetween(TimeSpan.Zero, TimeSpan.FromHours(1),
                    "extPriceSettings.MinOrderbooksSendingPeriod for " + _assetPairId);
                extPriceSettings.Markups.RequiredNotNull("extPriceSettings.Markups for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.RequiredNotNull("extPriceSettings.RepeatedOutliers for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxSequenceLength.RequiredBetween(2, 100000,
                    "extPriceSettings.RepeatedOutliers.MaxSequenceLength for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxSequenceAge.RequiredBetween(TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromHours(1), "extPriceSettings.RepeatedOutliers.MaxSequenceAge for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxAvg.RequiredBetween(0.0001m, 1,
                    "extPriceSettings.RepeatedOutliers.MaxAvg for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxAvgAge.RequiredBetween(TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromHours(1), "extPriceSettings.RepeatedOutliers.MaxAvgAge for " + _assetPairId);
                extPriceSettings.Steps.RequiredNotNull("extPriceSettings.Steps for " + _assetPairId);
                    
                extPriceSettings.Exchanges.RequiredNotNull(
                    "extPriceSettings.Exchanges for pair " + _assetPairId);
                foreach (var (exchangeName, exchangeSettings) in extPriceSettings.Exchanges)
                    new ExchangeValidator(_assetPairId, exchangeName, exchangeSettings).Validate();
            }

            private void Validate(CrossRateCalcInfo crossRateCalcInfo)
            {
                crossRateCalcInfo.RequiredNotNull("crossRateCalcInfo for pair " + _assetPairId);
                crossRateCalcInfo.ResultingPairId.RequiredNotNull(
                    "crossRateCalcInfo.ResultingPairId for pair " + _assetPairId);
                Validate(crossRateCalcInfo.Source1);
                Validate(crossRateCalcInfo.Source2);
            }

            private void Validate(CrossRateSourceAssetPair crossRateSourceAssetPair)
            {
                crossRateSourceAssetPair.RequiredNotNull("crossRateSourceAssetPair for pair " + _assetPairId);
                crossRateSourceAssetPair.Id.RequiredNotNull("crossRateSourceAssetPair.Id for pair " +
                                                            _assetPairId);
            }
        }

        private struct ExchangeValidator
        {
            private readonly string _assetPairId;
            private readonly string _exchangeName;
            private readonly ExchangeExtPriceSettings _exchangeSettings;

            public ExchangeValidator(string assetPairId, string exchangeName, ExchangeExtPriceSettings exchangeSettings)
            {
                _assetPairId = assetPairId;
                _exchangeName = exchangeName;
                _exchangeSettings = exchangeSettings;
            }

            public void Validate()
            {
                _exchangeSettings.RequiredNotNull(
                    $"_exchangeSettings for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.OrderbookOutdatingThreshold.RequiredBetween(TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromHours(1),
                    $"_exchangeSettings.OrderbookOutdatingThreshold for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.Disabled.RequiredNotNull(
                    $"_exchangeSettings.Disabled for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.Hedging.RequiredNotNull(
                    $"_exchangeSettings.Hedging for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.Hedging.DefaultPreference.RequiredBetween(0, 1,
                    $"_exchangeSettings.Hedging.DefaultPreference for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.OrderGeneration.RequiredNotNull(
                    $"_exchangeSettings.OrderGeneration for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.OrderGeneration.VolumeMultiplier.RequiredGreaterThan(0,
                    $"_exchangeSettings.OrderGeneration.VolumeMultiplier for pair {_assetPairId} and exchange {_exchangeName}");
                _exchangeSettings.OrderGeneration.VolumeMultiplier.RequiredGreaterThan(0,
                    $"_exchangeSettings.OrderGeneration.VolumeMultiplier for pair {_assetPairId} and exchange {_exchangeName}");
            }
        }
    }
}