using JetBrains.Annotations;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface ISettingsChangesAuditService
    {
        [CanBeNull] SettingsChangesAuditInfo GetAudit(SettingsRoot old, SettingsRoot changed);
    }
}