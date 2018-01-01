using System.Threading.Tasks;
using MarginTrading.MarketMaker.Messages;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IAlertService
    {
        void AlertPrimaryExchangeSwitched(PrimaryExchangeSwitchedMessage message);
        Task AlertRiskOfficer(string assetPairId, string message);
        void AlertStarted();
        Task AlertStopping();
        void StopOrAllowNewTrades(string assetPairId, string reason, bool stop);
    }
}