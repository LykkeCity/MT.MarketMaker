using System;
using FluentAssertions;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.AzureRepositories;

namespace Tests.Integrational
{
    internal class InMemoryBlobStorageSingleObjectFactory : IAzureBlobStorageFactoryService
    {
        public AzureBlobJsonSingleObjectStorageInMemory Blob { get; } = new AzureBlobJsonSingleObjectStorageInMemory();
        [CanBeNull] private string _connectionString;

        public IAzureBlobJsonStorage Create(IReloadingManager<string> connectionStringManager)
        {
            return Create(connectionStringManager.CurrentValue);
        }
        
        public AzureBlobJsonSingleObjectStorageInMemory Create(string connectionString)
        {
            if (_connectionString == null)
                _connectionString = connectionString;
            else
                connectionString.Should().Be(_connectionString);

            return Blob;
        }
    }
    
    internal class AzureBlobJsonSingleObjectStorageInMemory : IAzureBlobJsonStorage
    {
        private (string container, string key, Type)? _objectParams;
        public object Object { get; set; }

        public T GetObject<T>()
        {
            return (T) Object;
        }

        public T Read<T>(string container, string key) where T : class
        {
            ValidateObjectParams<T>(container, key);
            return GetObject<T>();
        }

        public void Write<T>(string container, string key, T obj) where T : class
        {
            ValidateObjectParams<T>(container, key);
            Object = obj;
        }

        private void ValidateObjectParams<T>(string container, string key) where T : class
        {
            var objectParams = (container, key, typeof(T));
            if (_objectParams == null)
                _objectParams = objectParams;
            else
                _objectParams.Should().Be(objectParams);
        }
    }
}