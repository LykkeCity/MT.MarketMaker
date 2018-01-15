using System;
using AutoMapper;
using AzureStorage;
using Common.Log;
using Lykke.AzureStorage.Tables;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.AzureRepositories.Implementation
{
    internal class SettingsChangesAuditRepository : ISettingsChangesAuditRepository
    {
        private readonly IConvertService _convertService;
        private readonly INoSQLTableStorage<SettingsChangesAuditEntity> _tableStorage;

        public SettingsChangesAuditRepository(IReloadingManager<MarginTradingMarketMakerSettings> settings, ILog log,
            IConvertService convertService, IAzureTableStorageFactoryService azureTableStorageFactoryService)
        {
            _convertService = convertService;
            _tableStorage = azureTableStorageFactoryService.Create<SettingsChangesAuditEntity>(
                settings.Nested(s => s.Db.ConnectionString), "MarketMakerChangesAuditLog", log);
        }

        public void Insert(SettingsChangesAuditInfo auditInfo)
        {
            var entity =
                _convertService.Convert<SettingsChangesAuditInfo, SettingsChangesAuditEntity>(auditInfo,
                    o => o.ConfigureMap(MemberList.Source));
            entity.PartitionKey = entity.DateTime.ToString("yyyy-MM-dd");
            entity.RowKey = entity.DateTime.ToString("HH:mm:ss.fffffff");
            _tableStorage.InsertAsync(entity).GetAwaiter().GetResult();
        }

        internal class SettingsChangesAuditEntity : AzureTableEntity
        {
            public DateTime DateTime { get; set; }
            public string RemoteIpAddress { get; set; }
            public string UserInfo { get; set; }
            public string Path { get; set; }
            public string Differences { get; set; }
        }
    }
}