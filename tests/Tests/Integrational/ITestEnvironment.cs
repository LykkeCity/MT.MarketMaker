using System;
using System.Linq.Expressions;
using Autofac;
using Moq;

namespace Tests.Integrational
{
    public interface ITestEnvironment
    {
        ITestEnvironment Setup<TMocked>(Expression<Func<TMocked, bool>> setup) where TMocked : class;
        ITestEnvironment Setup<TMocked>(params Action<Mock<TMocked>>[] setups) where TMocked : class;
        ITestEnvironment Setup(Action<ContainerBuilder> register);
        IContainer CreateContainer();
        ITestEnvironment Setup<TMocked>(TMocked instance) where TMocked : class;
    }
}