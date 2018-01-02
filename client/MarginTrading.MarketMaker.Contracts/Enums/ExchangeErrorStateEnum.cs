namespace MarginTrading.MarketMaker.Contracts.Enums
{
    public enum ExchangeErrorStateEnum
    {
        Valid = 0,
        Outlier = 1,
        Outdated = 2,
        Disabled = 4,
        Broken = 8,
    }
}