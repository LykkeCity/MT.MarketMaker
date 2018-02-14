using System;
using System.Net;

namespace MarginTrading.MarketMaker.Models
{
    public class SettingsChangesAuditInfo
    {
        public SettingsChangesAuditInfo(DateTime dateTime, IPAddress remoteIpAddress, string userInfo, string path, string differences)
        {
            DateTime = dateTime;
            RemoteIpAddress = remoteIpAddress;
            UserInfo = userInfo;
            Path = path;
            Differences = differences;
        }

        public DateTime DateTime { get; }
        public IPAddress RemoteIpAddress { get; }
        public string UserInfo { get; }
        public string Path { get; }
        public string Differences { get; }
    }
}