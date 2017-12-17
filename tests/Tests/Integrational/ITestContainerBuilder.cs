using System;
using System.Linq.Expressions;
using Autofac;
using Moq;

namespace Tests.Integrational
{
    public interface ITestContainerBuilder
    {
        ITestContainerBuilder Setup<TMocked>(Expression<Func<TMocked, bool>> setup) where TMocked : class;
        ITestContainerBuilder Setup<TMocked>(params Action<Mock<TMocked>>[] setups) where TMocked : class;
        IContainer CreateContainer();
        ITestContainerBuilder Setup<TMocked>(TMocked instance) where TMocked : class;
    }
}