﻿using System;
using System.Threading.Tasks;
using Common;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface IRabbitMqService
    {
        IMessageProducer<TMessage> GetProducer<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable);
        void Subscribe<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable, Func<TMessage, Task> handler);
    }
}