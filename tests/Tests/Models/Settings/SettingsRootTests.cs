using FluentAssertions;
using MarginTrading.MarketMaker.Models.Settings;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Models.Settings
{
    public class SettingsRootTests
    {
        [Test]
        public void Always_ShouldBeCorrectlyDeserialized()
        {
            //arrange
            var json = "{\"AssetPairs\":{\"XAUUSD\":{\"QuotesSourceType\":2,\"ExtPriceSettings\":{\"PresetDefaultExchange\":\"ICM\",\"OutlierThreshold\":0.02,\"MinOrderbooksSendingPeriod\":\"00:00:00.5000000\",\"Markups\":{\"Bid\":0.0,\"Ask\":0.0},\"RepeatedOutliers\":{\"MaxSequenceLength\":5,\"MaxSequenceAge\":\"00:01:00\",\"MaxAvg\":0.2,\"MaxAvgAge\":\"00:00:15\"},\"Steps\":{\"FindOutdated\":true,\"FindOutliers\":false,\"FindRepeatedProblems\":false,\"ChoosePrimary\":true,\"GetArbitrageFreeSpread\":false,\"Transform\":true},\"Exchanges\":{\"ICM\":{\"OrderbookOutdatingThreshold\":\"00:00:30\",\"Disabled\":{\"IsTemporarilyDisabled\":false,\"Reason\":\"\"},\"Hedging\":{\"DefaultPreference\":1.0,\"IsTemporarilyUnavailable\":false},\"OrderGeneration\":{\"VolumeMultiplier\":1.0,\"OrderRenewalDelay\":\"00:00:15\"}}}},\"CrossRateCalcInfo\":{\"ResultingPairId\":\"BTCCHF\",\"Source1\":{\"Id\":\"BTCUSD\",\"IsTransitoryAssetQuoting\":true},\"Source2\":{\"Id\":\"USDCHF\",\"IsTransitoryAssetQuoting\":false}}}}}";

            //act
            var result = JsonConvert.DeserializeObject<SettingsRoot>(json);

            //assert
            result.Should().NotBeNull();
            result.AssetPairs.Should().NotBeNull();
        }
    }
}