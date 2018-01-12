namespace MarginTrading.MarketMaker.Enums
{
    public enum OrderbookGeneratorStepDomainEnum
    {
        FindBroken = 20,
        FindOutdated = 30,
        FindOutliers = 40,
        FindRepeatedProblems = 50,
        ChoosePrimary = 60,
        GetArbitrageFreeSpread = 70,
        Transform = 80
    }
}