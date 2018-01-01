using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal static class Trace
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        private static readonly BlockingCollection<(string MsgGroup, string Msg, DateTime Time)> _writingQueue = new BlockingCollection<(string MsgGroup, string Msg, DateTime Time)>(10000);
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<(string Msg, DateTime Time)>> _lastElemsQueues
            = new ConcurrentDictionary<string, ConcurrentQueue<(string Msg, DateTime Time)>>();

        static Trace()
        {
            Task.Run(() =>
            {
                while (true)
                    foreach (var (assetPairId, msg, time) in _writingQueue.GetConsumingEnumerable())
                    {
                        var lastElemsQueue = _lastElemsQueues.GetOrAdd(assetPairId, k => new ConcurrentQueue<(string Msg, DateTime Time)>());
                        lastElemsQueue.Enqueue((msg, time));
                        if (lastElemsQueue.Count > 100) lastElemsQueue.TryDequeue(out var _);
                        if (TestFunctionalityFilter.TestsEnabled) Console.WriteLine(msg);
                        // todo: send rabbit messages
                    }
            });
        }

        public static void Write(string msgGroup, string msg)
        {
            _writingQueue.Add((msgGroup, msg, DateTime.Now));
        }

        public static void Write(string msgGroup, string msg, object obj)
        {
            Write(msgGroup, msg + ": " + JsonConvert.SerializeObject(obj, JsonSerializerSettings));
        }

        public static List<LogModel> GetLast()
        {
            return _lastElemsQueues.ToArray()
                .SelectMany(q => q.Value.ToArray().Select(t =>
                    new LogModel {Time = t.Time, Group = q.Key, Message = t.Msg}))
                .OrderByDescending(t => t.Time)
                .ToList();
        }
    }
}
