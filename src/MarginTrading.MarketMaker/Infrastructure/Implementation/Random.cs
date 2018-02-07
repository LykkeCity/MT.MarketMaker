namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal class Random : IRandom
    {
        private readonly System.Random _random = new System.Random();
        
        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}