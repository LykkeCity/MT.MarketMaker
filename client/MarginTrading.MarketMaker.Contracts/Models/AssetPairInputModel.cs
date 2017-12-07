using MarginTrading.MarketMaker.Contracts.Enums;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class AssetPairInputModel
    {
        public string AssetPairId { get; set; }
        public AssetPairQuotesSourceTypeEnum SourceType { get; set; }
    }
}