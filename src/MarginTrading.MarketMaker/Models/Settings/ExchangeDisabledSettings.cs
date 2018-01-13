using System;

namespace MarginTrading.MarketMaker.Models.Settings
{
    public class ExchangeDisabledSettings
    {
        public ExchangeDisabledSettings(bool isTemporarilyDisabled, string reason)
        {
            IsTemporarilyDisabled = isTemporarilyDisabled;
            Reason = reason;
        }

        public bool IsTemporarilyDisabled { get; }
        public string Reason { get; }
    }
}