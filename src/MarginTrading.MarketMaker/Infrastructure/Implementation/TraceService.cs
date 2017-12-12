using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.Filters;
using MarginTrading.MarketMaker.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    public interface ITraceService
    {
        void Write(TraceGroupEnum group, string assetPairId, string msg, object obj);
        List<TraceModel> GetLast();
        List<TraceModel> GetLast(string contains);
    }

    internal class TraceService : ITraceService, IStartable
    {
        private static readonly BlockingCollection<TraceMessage> WritingQueue =
            new BlockingCollection<TraceMessage>(10000);

        private static readonly ConcurrentDictionary<(TraceGroupEnum Group, string AssetPairId), ConcurrentQueue<TraceMessage>> LastElemsQueues
            = new ConcurrentDictionary<(TraceGroupEnum, string), ConcurrentQueue<TraceMessage>>();

        private readonly ISystem _system;
        private readonly IMessageProducer<TraceMessage> _messageProducer;

        public TraceService(ISystem system, IRabbitMqService rabbitMqService,
            IReloadingManager<MarginTradingMarketMakerSettings> settings)
        {
            _system = system;
            _messageProducer =
                rabbitMqService.GetProducer<TraceMessage>(settings.Nested(s => s.RabbitMq.Publishers.Trace), true, false);
        }

        public void Start()
        {
            Trace.TraceService = this;
            Task.Run(() =>
            {
                while (true)
                    foreach (var m in WritingQueue.GetConsumingEnumerable())
                    {
                        Console.WriteLine(m.AssetPairId + '\t' + m.Group + '\t' + m.Msg);
                        _messageProducer.ProduceAsync(m);

                        var lastElemsQueue =
                            LastElemsQueues.GetOrAdd((m.Group, m.AssetPairId), k => new ConcurrentQueue<TraceMessage>());
                        lastElemsQueue.Enqueue(m);
                        
                        while (lastElemsQueue.Count > 100) 
                            lastElemsQueue.TryDequeue(out var _);
                    }
            });
        }

        public void Write(TraceGroupEnum group, string assetPairId, string msg, object obj)
        {
            WritingQueue.Add(new TraceMessage(group, assetPairId, msg, obj, _system.UtcNow));
        }

        public List<TraceModel> GetLast()
        {
            return GetLastCore().OrderByDescending(t => t.Time).ToList();
        }

        public List<TraceModel> GetLast(string contains)
        {
            return GetLastCore().Where(l => Contains(l.Group + '\t' + l.Message, contains))
                .OrderByDescending(t => t.Time).ToList();
        }

        private static IEnumerable<TraceModel> GetLastCore()
        {
            return LastElemsQueues.ToArray()
                .SelectMany(q =>
                {
                    var gr = q.Key.AssetPairId + '\t' + q.Key.Group;
                    return q.Value.ToArray().Select(m => new TraceModel
                    {
                        Time = m.Time,
                        Group = gr,
                        Message = m.Msg,
                        AssetPairId = q.Key.AssetPairId,
                        TraceGroup = q.Key.Group.ToString(),
                        Data = m.Data,
                    });
                });
        }

        private static bool Contains(string text, string contains)
        {
            return contains.Split(" OR ")
                .Any(containsOr =>
                    containsOr.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(c =>
                        text.IndexOf(c.Trim(), StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public class TraceMessage
        {
            public TraceGroupEnum Group { get; }
            public string AssetPairId { get; }
            public string Msg { get; }
            public object Data { get; }
            public DateTime Time { get; }

            public TraceMessage(TraceGroupEnum group, string assetPairId, string msg, object data, DateTime time)
            {
                Group = group;
                AssetPairId = assetPairId;
                Msg = msg;
                Data = data;
                Time = time;
            }
        }
    }
}