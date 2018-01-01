using System;
using System.Threading.Tasks;
using Lykke.Service.CandlesHistory.Client;
using Lykke.Service.CandlesHistory.Client.Models;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Services.AvgSpotRates.Implementation;
using MarginTrading.MarketMaker.Services.Common;
using NUnit.Framework;
using Moq;

namespace Tests.Services.AvgSpotRates
{
    public class AvgSpotRatesServiceTests
    {
        private static readonly TestSuit<AvgSpotRatesService> _testSuit = TestSuit.Create<AvgSpotRatesService>();


        [SetUp]
        public void SetUp()
        {
            _testSuit.Reset();
        }

        [Test]
        public async Task Always_ShouldSendRates()
        {
            //arrange
            var history = new CandlesHistoryResponseModel
            {
                History = new[] {new Candle {Open = 10, Close = 20}, new Candle {Open = 30, Close = 40}}
            };
            var now = DateTime.UtcNow;
            _testSuit
                .Setup<ISystem>(s => s.UtcNow == now)
                .Setup<ICandleshistoryservice>(s =>
                    s.GetCandlesHistoryOrErrorWithHttpMessagesAsync("LKKUSD", PriceType.Mid, TimeInterval.Min5,
                        now.AddHours(-12), now, null, default) == history.ToResponse<object>())
                .Setup<IMarketMakerService>(s => s.ProcessNewAvgSpotRate("LKKUSD", 25, 25) == Task.CompletedTask);

            //act
            await _testSuit.Sut.Execute();

            //assert
            _testSuit.GetMock<IMarketMakerService>().Verify(s => s.ProcessNewAvgSpotRate("LKKUSD", 25, 25), Times.Once);
        }
    }
}