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

        public ITestContainerBuilder Build()
        {
            Reset();
            return GetTestContainerBuilder();
        }

        public IntegrationalTestSuit WithModule(IModule module)
        {
            _modules.Add(module);
            return this;
        }

        protected virtual TestContainerBuilder GetTestContainerBuilder()
        {
            return new TestContainerBuilder(this);
        }


        protected class TestContainerBuilder : ITestContainerBuilder
        {
            private readonly IntegrationalTestSuit _suit;
            private readonly List<(object Instance, Type Type)> _instancesToInject = new List<(object, Type)>();

            public TestContainerBuilder(IntegrationalTestSuit suit)
            {
                _suit = suit;
            }

            public ITestContainerBuilder Setup<TMocked>(TMocked instance) where TMocked : class
            {
                _instancesToInject.Add((instance, typeof(TMocked)));
                return this;
            }

            public ITestContainerBuilder Setup<TMocked>(Expression<Func<TMocked, bool>> setup) where TMocked : class
            {
                return Setup(_suit.SetupCore(setup).Object);
            }

            public ITestContainerBuilder Setup<TMocked>(params Action<Mock<TMocked>>[] setups) where TMocked : class
            {
                return Setup(_suit.SetupCore(setups).Object);
            }

            public IContainer CreateContainer()
            {
                var builder = new ContainerBuilder();
                foreach (var module in _suit._modules)
                    builder.RegisterModule(module);

                foreach (var mock in _instancesToInject)
                    builder.RegisterInstance(mock.Instance).As(mock.Type).SingleInstance();

                return builder.Build();
            }
        }
    }
}