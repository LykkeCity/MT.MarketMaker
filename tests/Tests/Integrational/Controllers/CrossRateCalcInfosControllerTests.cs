using Autofac;
using FluentAssertions;
using MarginTrading.MarketMaker.Controllers;
using NUnit.Framework;

namespace Tests.Integrational.Controllers
{
    public class CrossRateCalcInfosControllerTests
    {
        private readonly MmIntegrationalTestSuit _testSuit = new MmIntegrationalTestSuit();
        
        [Test]
        public void IfCrossRateCalcInfoNotExists_ShouldGetNull()
        {
            //arrange
            var env = _testSuit.Build();
            env.Setup(b => b.RegisterType<CrossRateCalcInfosController>().AsSelf());
            var container = env.CreateContainer();
            var crossRateCalcInfosController = container.Resolve<CrossRateCalcInfosController>();

            //act
            var info = crossRateCalcInfosController.Get("non-existent pair");
            
            //assert
            info.Should().BeNull();
        }
    }
}