using System;
using System.Collections.Immutable;
using MarginTrading.MarketMaker.Enums;
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
                new AssetPairValidator(assetPairId, pairSettings, root).Validate();
        }

        private struct AssetPairValidator
        {
            private readonly string _assetPairId;
            private readonly AssetPairSettings _pairSettings;
            private readonly SettingsRoot _root;

            public AssetPairValidator(string assetPairId, AssetPairSettings pairSettings, SettingsRoot root)
            {
                _assetPairId = assetPairId;
                _pairSettings = pairSettings;
                _root = root;
            }

            public void Validate()
            {
                _pairSettings.RequiredNotNull("_pairSettings for pair " + _assetPairId);
                _pairSettings.QuotesSourceType.RequiredEnum("_pairSettings.QuotesSourceType for pair " + _assetPairId);
                switch (_pairSettings.QuotesSourceType)
                {
                    case AssetPairQuotesSourceTypeDomainEnum.External:
                        Validate(_pairSettings.ExtPriceSettings);

                        if (_pairSettings.ExtPriceSettings.Steps.GetValueOrDefault(
                            OrderbookGeneratorStepDomainEnum.AggregateOrderbook, true))
                        {
                            Validate(_pairSettings.AggregateOrderbookSettings);
                        }

                        break;
                    case AssetPairQuotesSourceTypeDomainEnum.CrossRates:
                        Validate(_pairSettings.CrossRateCalcInfo);
                        break;
                }
            }

            private void Validate(AggregateOrderbookSettings aggregateOrderbookSettings)
            {
                aggregateOrderbookSettings.RequiredNotNull("aggregateOrderbookSettings for pair " + _assetPairId);
                aggregateOrderbookSettings.AsIsLevelsCount.RequiredNotLessThan(0,
                    "aggregateOrderbookSettings.AsIsLevelsCount for pair " + _assetPairId);
                var assetPairId = _assetPairId;
                aggregateOrderbookSettings.CumulativeVolumeLevels.RequiredAll(v =>
                    v.RequiredGreaterThan(0, 
                        "aggregateOrderbookSettings.CumulativeVolumeLevels for pair " + assetPairId));
                aggregateOrderbookSettings.RandomFraction.RequiredBetween(0, 0.5m,
                    "aggregateOrderbookSettings.RandomFraction for pair " + _assetPairId);

                if (aggregateOrderbookSettings.AsIsLevelsCount == 0 &&
                    aggregateOrderbookSettings.CumulativeVolumeLevels.IsEmpty)
                {
                    throw new ArgumentException("Either AsIsLevelsCount or CumulativeVolumeLevels should be filled",
                        "aggregateOrderbookSettings for pair " + _assetPairId);
                }
            }

            private void Validate(AssetPairExtPriceSettings extPriceSettings)
            {
                extPriceSettings.RequiredNotNull("extPriceSettings for pair " + _assetPairId);

                extPriceSettings.Exchanges.RequiredNotNullOrEmpty(
                    "extPriceSettings.Exchanges for pair " + _assetPairId);

                extPriceSettings.PresetDefaultExchange.RequiredInSet(extPriceSettings.Exchanges.Keys,
                    "extPriceSettings.PresetDefaultExchange for pair " + _assetPairId);
                extPriceSettings.OutlierThreshold.RequiredBetween(0.00001m, 0.5m,
                    "extPriceSettings.OutlierThreshold for " + _assetPairId);
                extPriceSettings.MinOrderbooksSendingPeriod.RequiredBetween(TimeSpan.Zero, TimeSpan.FromHours(1),
                    "extPriceSettings.MinOrderbooksSendingPeriod for " + _assetPairId);
                extPriceSettings.Markups.RequiredNotNull("extPriceSettings.Markups for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.RequiredNotNull(
                    "extPriceSettings.RepeatedOutliers for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxSequenceLength.RequiredBetween(2, 100000,
                    "extPriceSettings.RepeatedOutliers.MaxSequenceLength for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxSequenceAge.RequiredBetween(TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromHours(1), "extPriceSettings.RepeatedOutliers.MaxSequenceAge for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxAvg.RequiredBetween(0.0001m, 1,
                    "extPriceSettings.RepeatedOutliers.MaxAvg for " + _assetPairId);
                extPriceSettings.RepeatedOutliers.MaxAvgAge.RequiredBetween(TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromHours(1), "extPriceSettings.RepeatedOutliers.MaxAvgAge for " + _assetPairId);
                extPriceSettings.Steps.RequiredNotNull("extPriceSettings.Steps for " + _assetPairId);

                foreach (var (exchangeName, exchangeSettings) in extPriceSettings.Exchanges)
                    new ExchangeValidator(_assetPairId, exchangeName, exchangeSettings).Validate();
            }

            private void Validate(CrossRateCalcInfo crossRateCalcInfo)
            {
                crossRateCalcInfo.RequiredNotNull("crossRateCalcInfo for pair " + _assetPairId);
                crossRateCalcInfo.ResultingPairId.RequiredEqualsTo(_assetPairId,
                    "crossRateCalcInfo.ResultingPairId for pair " + _assetPairId);
                Validate(crossRateCalcInfo.Source1);
                Validate(crossRateCalcInfo.Source2);
                crossRateCalcInfo.Source1.Id.RequiredNotEqualsTo(crossRateCalcInfo.Source2.Id,
                    "crossRateCalcInfo.Source1.Id equals crossRateCalcInfo.Source2.Id for pair " + _assetPairId);
            }

            private void Validate(CrossRateSourceAssetPair crossRateSourceAssetPair)
            {
                crossRateSourceAssetPair.RequiredNotNull("crossRateSourceAssetPair for pair " + _assetPairId);
                crossRateSourceAssetPair.Id.RequiredInSet(_root.AssetPairs.Keys,
                    "crossRateSourceAssetPair.Id for pair " + _assetPairId);
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
                _exchangeSettings.OrderbookOutdatingThreshold.RequiredBetween(
                    TimeSpan.FromMilliseconds(10), TimeSpan.FromHours(1),
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
                _exchangeSettings.OrderGeneration.OrderRenewalDelay.RequiredBetween(TimeSpan.Zero,
                    TimeSpan.FromHours(1),
                    $"_exchangeSettings.OrderGeneration.OrderRenewalDelay for pair {_assetPairId} and exchange {_exchangeName}");
            }
        }
    }
}