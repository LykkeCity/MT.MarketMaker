using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.AzureRepositories
{
    internal interface ISettingsChangesAuditRepository
    {
        void Insert(SettingsChangesAuditInfo auditInfo);
    }
}