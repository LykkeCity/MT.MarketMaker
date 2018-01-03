﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Lykke.Logs;
using Lykke.SlackNotifications;
using MarginTrading.MarketMaker.Contracts.Models;
using MarginTrading.MarketMaker.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal class TraceService : ITraceService, ICustomStartup
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new StringEnumConverter()}
        };

        private static readonly BlockingCollection<TraceMessage> WritingQueue =
            new BlockingCollection<TraceMessage>(50000);

        private static readonly
            ConcurrentDictionary<(TraceLevelGroupEnum Group, string AssetPairId), ConcurrentQueue<TraceMessage>>
            LastElemsQueues
                = new ConcurrentDictionary<(TraceLevelGroupEnum, string), ConcurrentQueue<TraceMessage>>();

        private readonly ISystem _system;
        private readonly LykkeLogToAzureStorage _logToAzureStorage;
        private readonly ISlackNotificationsSender _slackNotificationsSender;

        public TraceService(ISystem system,
            LykkeLogToAzureStorage logToAzureStorage,
            ISlackNotificationsSender slackNotificationsSender)
        {
            _system = system;
            _logToAzureStorage = logToAzureStorage;
            _slackNotificationsSender = slackNotificationsSender;
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
                            var lastElemsQueue =
                                LastElemsQueues.GetOrAdd((m.TraceGroup, m.AssetPairId),
                                    k => new ConcurrentQueue<TraceMessage>());
                            lastElemsQueue.Enqueue(m);

                            while (lastElemsQueue.Count > 200)
                                lastElemsQueue.TryDequeue(out var _);

                            var message = m.AssetPairId + '\t' + m.TraceGroup + '\t' + m.Msg;
                            Console.WriteLine(message);
                            _logToAzureStorage.WriteInfoAsync("MtMmTrace",
                                JsonConvert.SerializeObject(m, JsonSerializerSettings), message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _slackNotificationsSender.SendErrorAsync(e.ToAsyncString());
                    }
            });
        }

        public void Write(TraceLevelGroupEnum levelGroup, string assetPairId, string msg, object obj)
        {
            if (!WritingQueue.TryAdd(new TraceMessage(levelGroup, assetPairId, msg, obj, _system.UtcNow)))
                Console.WriteLine("ERROR WRITING TO TRACE QUEUE:\t" + assetPairId + '\t' + levelGroup + '\t' + msg);
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
                    var gr = q.Key.AssetPairId + ' ' + q.Key.Group;
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
            public TraceLevelGroupEnum TraceGroup { get; }
            public string AssetPairId { get; }
            public string Msg { get; }
            public object Data { get; }
            public DateTime Time { get; }

            public TraceMessage(TraceLevelGroupEnum traceGroup, string assetPairId, string msg, object data,
                DateTime time)
            {
                TraceGroup = traceGroup;
                AssetPairId = assetPairId;
                Msg = msg;
                Data = data;
                Time = time;
            }
        }
    }
}