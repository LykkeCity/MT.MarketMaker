using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Infrastructure;
using MarginTrading.MarketMaker.Settings;

namespace Tests.Integrational
{
    public class StubRabbitMqService : IRabbitMqService
    {
        private readonly ConcurrentDictionary<Type, StubRabbitMqPublisher> _producers = new ConcurrentDictionary<Type, StubRabbitMqPublisher>();

        public List<TMessage> GetSentMessages<TMessage>()
        {
            return GetProducer<TMessage>().SentMessages.Cast<TMessage>().ToList();
        }

        public IMessageProducer<TMessage> GetProducer<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable, bool useMessagePack)
        {
            return (IMessageProducer<TMessage>) GetProducer<TMessage>();
        }

        public void Subscribe<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable, Func<TMessage, Task> handler)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            _producers.Clear();
        }

        private StubRabbitMqPublisher GetProducer<TMessage>()
        {
            return _producers.GetOrAdd(typeof(TMessage), new StubRabbitMqPublisher<TMessage>());
        }

        private class StubRabbitMqPublisher
        {
            public ConcurrentQueue<object> SentMessages { get; } = new ConcurrentQueue<object>();
        }

        private class StubRabbitMqPublisher<T> : StubRabbitMqPublisher, IMessageProducer<T>
        {
            public Task ProduceAsync(T message)
            {
                SentMessages.Enqueue(message);
                return Task.CompletedTask;
            }
        }
    }
}