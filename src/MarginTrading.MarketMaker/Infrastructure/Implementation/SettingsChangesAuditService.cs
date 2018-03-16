using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Aqua.GraphCompare;
using Common;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;
using Microsoft.AspNetCore.Http;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal class SettingsChangesAuditService : ISettingsChangesAuditService
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new StringEnumConverter()}
        };

        private readonly ISystem _system;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingsChangesAuditService(ISystem system, IHttpContextAccessor httpContextAccessor)
        {
            _system = system;
            _httpContextAccessor = httpContextAccessor;
        }

        public SettingsChangesAuditInfo GetAudit(SettingsRoot old, SettingsRoot changed)
        {
            var diff = new GraphComparer(GetDisplayString).Compare(old, changed);
            if (diff.IsMatch)
                return null;

            var diffStr = JsonConvert.SerializeObject(
                diff.Deltas.GroupBy(d => FormatBreadcrump(d.Breadcrumb))
                    .Select(GetChangeDescriptor)
                    .Where(d => d.Key != null)
                    .ToDictionary(),
                JsonSerializerSettings);

            var httpContext = _httpContextAccessor.HttpContext;
            return new SettingsChangesAuditInfo(_system.UtcNow, httpContext?.Connection.RemoteIpAddress,
                httpContext?.Request.Headers["User-Info"].ToDelimitedString(", "), $"{httpContext?.Request.Method} {httpContext?.Request.Path}", diffStr);
        }

        private static KeyValuePair<string, object[]> GetChangeDescriptor(IGrouping<string, Delta> group)
        {
            var deltas = group.ToList();
            Delta delta = null;
            switch (deltas.Count)
            {
                case 1:
                    delta = deltas.First();
                    break;
                case 2:
                    var first = deltas.First();
                    var second = deltas.Last();
                    if (JsonConvert.SerializeObject(first.OldValue) == JsonConvert.SerializeObject(second.NewValue) ||
                        JsonConvert.SerializeObject(first.NewValue) == JsonConvert.SerializeObject(second.OldValue))
                    {
                        return new KeyValuePair<string, object[]>(null, null);
                    }
                    
                    break;
            }

            if (delta == null)
                throw new InvalidOperationException("Cannot figure out what was changed: " + deltas.ToJson());
                    
            return KeyValuePair.Create(group.Key, new[] {delta.ChangeType, delta.OldValue, delta.NewValue});
        }

        private static string FormatBreadcrump(Breadcrumb breadcrumb)
        {
            var results = new List<string>();
            do
            {
                results.Add(breadcrumb.DisplayString);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            } while ((breadcrumb = breadcrumb.Parent) != null);

            results.Reverse();
            return results.ToDelimitedString(".");
        }

        private static string GetDisplayString(object item, PropertyInfo prop)
        {
            var type = item.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                return '[' + ((dynamic) item).Key.ToString() + ']';

            var displayString = prop?.Name;
            return string.IsNullOrEmpty(displayString) ? "Root" : displayString;
        }
    }
}