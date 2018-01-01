using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests
{
    public class TestSuitTests
    {
        private static readonly TestSuit<MySut> _testSuit = TestSuit.Create<MySut>();

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        [Test]
        public void Reset_SutShouldBeRecreated()
        {
            //arrange
            _testSuit.Sut.I = 10;

            //act
            _testSuit.Reset();

            //assert
            _testSuit.Sut.I.Should().Be(1);
        }

        [Test]
        public void UsingTraditionalSetups_ShouldCorrectlySetupMocks()
        {
            //arrange

            //act
            _testSuit
                .Setup<IDependency1>(d => d.Setup(m => m.Action1()).Returns("first"))
                .Setup<IDependency2>(d => d.Setup(m => m.Action2()).Returns("second"));

            //assert
            _testSuit.Sut.Dependency1.Action1().Should().Be("first");
            _testSuit.Sut.Dependency2.Action2().Should().Be("second");
        }


        [Test]
        public void UsingExpressionSetups_ShouldCorrectlySetupMocks()
        {
            //arrange

            //act
            _testSuit
                .Setup<IDependency1>(d => d.Action1() == "first")
                .Setup<IDependency2>(d => d.Action2() == "second");

            //assert
            _testSuit.Sut.Dependency1.Action1().Should().Be("first");
            _testSuit.Sut.Dependency2.Action2().Should().Be("second");
        }

        [Test]
        public void Always_ShouldCacheMocks()
        {
            //arrange

            //act

            //assert
            _testSuit.Sut.Dependency1.Should().Be(_testSuit.GetMockObj<IDependency1>());
            _testSuit.Sut.Dependency2.Should().Be(_testSuit.GetMockObj<IDependency2>());
        }

        [Test]
        public void Always_ShouldCacheSut()
        {
            //arrange

            //act
            var suit = TestSuit.Create<MySut>();
            var sut1 = suit.Sut;
            var sut2 = suit.Sut;

            //assert
            sut1.Should().Be(sut2);
        }


        [UsedImplicitly]
        private class MySut
        {
            public int I { get; set; } = 1;
            public IDependency1 Dependency1 { get; }
            public IDependency2 Dependency2 { get; }

            public MySut(IDependency1 dependency1, IDependency2 dependency2)
            {
                Dependency1 = dependency1;
                Dependency2 = dependency2;
            }
        }

        public interface IDependency1
        {
            string Action1();
        }

        public interface IDependency2
        {
            string Action2();
        }
    }
}
