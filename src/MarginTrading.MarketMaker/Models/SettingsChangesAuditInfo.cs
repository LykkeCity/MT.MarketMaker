using System.Net;

namespace MarginTrading.MarketMaker.Models
{
    public class SettingsChangesAuditInfo
    {
        public SettingsChangesAuditInfo(IPAddress remoteIpAddress, string userInfo, string path, string differences)
        {
            RemoteIpAddress = remoteIpAddress;
            UserInfo = userInfo;
            Path = path;
            Differences = differences;
        }

        public IPAddress RemoteIpAddress { get; }
        public string UserInfo { get; }
        public string Path { get; }
        public string Differences { get; }
    }
}