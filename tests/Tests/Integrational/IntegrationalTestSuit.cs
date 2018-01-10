using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;
using Autofac.Core;
using Moq;

namespace Tests.Integrational
{
    public class IntegrationalTestSuit : TestSuitBase
    {
        private readonly List<IModule> _modules = new List<IModule>();

        public ITestEnvironment Build()
        {
            Reset();
            return GetTestContainerBuilder();
        }

        public IntegrationalTestSuit WithModule(IModule module)
        {
            _modules.Add(module);
            return this;
        }

        protected virtual TestEnvironment GetTestContainerBuilder()
        {
            return new TestEnvironment(this);
        }


        protected class TestEnvironment : ITestEnvironment
        {
            private readonly IntegrationalTestSuit _suit;
            private readonly List<Action<ContainerBuilder>> _builders = new List<Action<ContainerBuilder>>();  

            public TestEnvironment(IntegrationalTestSuit suit)
            {
                _suit = suit;
            }

            public ITestEnvironment Setup<TMocked>(TMocked instance) where TMocked : class
            {
                return Setup(b => b.RegisterInstance(instance).As(typeof(TMocked)).SingleInstance());
            }

            public ITestEnvironment Setup<TMocked>(Expression<Func<TMocked, bool>> setup) where TMocked : class
            {
                return Setup(_suit.SetupCore(setup).Object);
            }

            public ITestEnvironment Setup<TMocked>(params Action<Mock<TMocked>>[] setups) where TMocked : class
            {
                return Setup(_suit.SetupCore(setups).Object);
            }

            public ITestEnvironment Setup(Action<ContainerBuilder> register)
            {
                _builders.Add(register);
                return this;
            }

            public virtual IContainer CreateContainer()
            {
                var builder = new ContainerBuilder();
                foreach (var module in _suit._modules)
                    builder.RegisterModule(module);

                foreach (var b in _builders)
                    b(builder);

                return builder.Build();
            }
        }
    }
}