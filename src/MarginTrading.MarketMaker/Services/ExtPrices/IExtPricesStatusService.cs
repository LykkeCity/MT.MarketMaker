using System.Collections.Generic;
using System.Linq;
using MarginTrading.MarketMaker.Models.Api;

namespace MarginTrading.MarketMaker.Services.ExtPrices
{
    public interface IExtPricesStatusService
    {
        IReadOnlyList<ExtPriceStatusModel> Get();
        IReadOnlyList<ExtPriceStatusModel> Get(string assetPairId);
    }
}