using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace MarginTrading.MarketMaker.Infrastructure.Implemetation
{
    internal static class Trace
    {
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
                        if (lastElemsQueue.Count > 100)
                            lastElemsQueue.TryDequeue(out var _);

                        Console.WriteLine(msg);
                    }
            });
        }

        public static void Write(string msgGroup, string msg)
        {
            _writingQueue.Add((msgGroup, msg, DateTime.Now));
        }

        public static void Write(string msgGroup, string msg, object obj)
        {
            Write(msgGroup, msg + ": " + obj.ToJson());
        }

        public static IReadOnlyList<string> GetLast()
        {
            return _lastElemsQueues.ToArray()
                .SelectMany(q => q.Value.ToArray().Select(t => (q.Key, t.Msg, t.Time)))
                .OrderByDescending(t => t.Time)
                .Select(t => $"{t.Time:s}\t{t.Key}\t{t.Msg}")
                .ToList();
        }
    }
}
