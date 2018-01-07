using System;
using System.Threading.Tasks;
using Lykke.SettingsReader;

namespace Tests.Integrational
{
    public class StubReloadingManager<T> : IReloadingManager<T>
    {
        private readonly Func<T> _valueGetter;

        public StubReloadingManager(Func<T> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public Task<T> Reload()
        {
            return Task.FromResult(CurrentValue);
        }

        public bool HasLoaded => true;
        public T CurrentValue => _valueGetter();
    }
}