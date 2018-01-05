using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private int _counter;

        public List<TMessage> GetSentMessages<TMessage>()
        {
            return GetProducer<TMessage>().SentMessages.Select(m => m.Message).Cast<TMessage>().ToList();
        }
        
        public IReadOnlyList<object> GetSentMessages()
        {
            return _producers.ToArray().SelectMany(p => p.Value.SentMessages).OrderBy(w => w.Counter)
                .Select(w => w.Message).ToList();
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
            _counter = 0;
        }

        private StubRabbitMqPublisher GetProducer<TMessage>()
        {
            return _producers.GetOrAdd(typeof(TMessage), new StubRabbitMqPublisher<TMessage>(this));
        }

        private class StubRabbitMqPublisher
        {
            public ConcurrentQueue<SentMessageWrapper> SentMessages { get; } = new ConcurrentQueue<SentMessageWrapper>();
        }

        private class StubRabbitMqPublisher<T> : StubRabbitMqPublisher, IMessageProducer<T>
        {
            private readonly StubRabbitMqService _stubRabbitMqService;

            public StubRabbitMqPublisher(StubRabbitMqService stubRabbitMqService)
            {
                _stubRabbitMqService = stubRabbitMqService;
            }

            public Task ProduceAsync(T message)
            {
                SentMessages.Enqueue(new SentMessageWrapper(message, Interlocked.Increment(ref _stubRabbitMqService._counter)));
                return Task.CompletedTask;
            }
        }

        public class SentMessageWrapper
        {
            public SentMessageWrapper(object message, int counter)
            {
                Message = message ?? throw new ArgumentNullException(nameof(message));
                Counter = counter;
            }

            public object Message { get; }
            public int Counter { get; }
        }
    }
}