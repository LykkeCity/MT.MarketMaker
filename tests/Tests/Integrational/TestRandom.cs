using System;
using MarginTrading.MarketMaker.Infrastructure;

namespace Tests.Integrational
{
    internal class TestRandom : IRandom
    {
        public Generator<double> Doubles { get; set; } = Generate.FromLambda<double>((last, i) =>
        {
            switch (i % 3)
            {
                case 0:
                    return 0;
                case 1:
                    return 0.5;
                case 2:
                    // The max possible value. See https://msdn.microsoft.com/en-us/library/system.random.nextdouble(v=vs.110).aspx
                    return 0.99999999999999978;
                default:
                    throw new Exception("Bug");
            }
        });
        
        public double NextDouble()
        {
            return Doubles.Next();
        }
    }
}