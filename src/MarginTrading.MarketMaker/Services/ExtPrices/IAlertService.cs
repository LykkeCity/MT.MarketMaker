using System.Threading.Tasks;
using MarginTrading.MarketMaker.Contracts.Messages;
using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IAlertService
    {
        void AlertPrimaryExchangeSwitched(PrimaryExchangeSwitchedMessage message);
        void AlertRiskOfficer(string assetPairId, string message, EventTypeEnum eventType);
        void AlertStarted();
        Task AlertStopping();
        void StopOrAllowNewTrades(string assetPairId, string reason, bool stop);
    }
}