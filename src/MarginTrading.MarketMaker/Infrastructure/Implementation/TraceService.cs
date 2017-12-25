using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    internal class TraceService : ITraceService, ICustomStartup
    {
        private long _counter;
        
        private readonly BlockingCollection<TraceMessage> WritingQueue =
            new BlockingCollection<TraceMessage>(10000);

        private readonly ConcurrentDictionary<(TraceLevelGroupEnum Group, string AssetPairId), ConcurrentQueue<TraceMessage>> LastElemsQueues
            = new ConcurrentDictionary<(TraceLevelGroupEnum, string), ConcurrentQueue<TraceMessage>>();

        private readonly ISystem _system;
        private readonly IMessageProducer<TraceMessage> _messageProducer;

        public TraceService(ISystem system, IRabbitMqService rabbitMqService,
            IReloadingManager<MarginTradingMarketMakerSettings> settings)
        {
            _system = system;
            _messageProducer =
                rabbitMqService.GetProducer<TraceMessage>(settings.Nested(s => s.RabbitMq.Publishers.Trace), true, true);
        }
        
        public void Initialize()
        {
            Task.Run(() =>
            {
                while (true)
                    try
                    {
                        foreach (var m in WritingQueue.GetConsumingEnumerable())
                        {
                            if (m.Level < TraceLevelGroupEnum.Info)
                                Console.WriteLine(m.AssetPairId + '\t' + m.Level + '\t' + m.Msg);
                        
                            _messageProducer.ProduceAsync(m);

                            var lastElemsQueue =
                                LastElemsQueues.GetOrAdd((m.Level, m.AssetPairId), k => new ConcurrentQueue<TraceMessage>());
                            lastElemsQueue.Enqueue(m);
                        
                            while (lastElemsQueue.Count > 100) 
                                lastElemsQueue.TryDequeue(out var _);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
            });
        }

        public void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj)
        {
            var id = Interlocked.Increment(ref _counter); 
            WritingQueue.Add(new TraceMessage(id, levelGroup, assetPairId, msg, obj, _system.UtcNow));
        }

        public List<TraceModel> GetLast()
        {
            return GetLastCore().OrderByDescending(t => t.Time).ThenBy(t => t.Id).ToList();
        }

        public List<TraceModel> GetLast(string contains)
        {
            return GetLastCore().Where(l => Contains(l.Group + '\t' + l.Message, contains))
                .OrderByDescending(t => t.Time).ThenBy(t => t.Id).ToList();
        }

        private IEnumerable<TraceModel> GetLastCore()
        {
            return LastElemsQueues.ToArray()
                .SelectMany(q =>
                {
                    var gr = q.Key.AssetPairId + ' ' + q.Key.Group;
                    return q.Value.ToArray().Select(m => new TraceModel
                    {
                        Id = m.Id,
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
            public long Id { get; }
            public TraceLevelGroupEnum Level { get; }
            public string AssetPairId { get; }
            public string Msg { get; }
            public object Data { get; }
            public DateTime Time { get; }

            public TraceMessage(long id, TraceLevelGroupEnum level, string assetPairId, string msg, object data, DateTime time)
            {
                Level = level;
                AssetPairId = assetPairId;
                Msg = msg;
                Data = data;
                Time = time;
                Id = id;
            }
        }
    }
}