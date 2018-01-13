using System;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal class Factory<T>
    {
        private readonly IServiceProvider _serviceProvider;

        public Factory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}