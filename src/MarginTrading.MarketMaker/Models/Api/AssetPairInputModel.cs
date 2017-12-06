using MarginTrading.MarketMaker.Enums;

namespace MarginTrading.MarketMaker.Models.Api
{
    public class AssetPairInputModel
    {
        public string AssetPairId { get; set; }
        public AssetPairQuotesSourceTypeEnum SourceType { get; set; }
    }
}