using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
            [CanBeNull] private IContainer _container;

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
                if (_container != null) return _container;
                var builder = new ContainerBuilder();
                builder.Populate(new ServiceCollection()); // this registers an IServiceProvider
                foreach (var module in _suit._modules)
                    builder.RegisterModule(module);

                foreach (var b in _builders)
                    b(builder);

                _container = builder.Build();
                return _container;
            }
        }
    }
}