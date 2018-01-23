using System;

namespace MarginTrading.MarketMaker.Enums
{
    public enum OrderbookGeneratorStepDomainEnum
    {
        FindBroken = 20,
        FindOutdated = 30,
        FindOutliers = 40,
        FindRepeatedProblems = 50,
        ChoosePrimary = 60,
        [Obsolete("Step removed")]
        GetArbitrageFreeSpread = 70,
        Transform = 80
    }
}