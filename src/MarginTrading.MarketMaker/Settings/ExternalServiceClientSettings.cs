﻿using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    public class ExternalServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
