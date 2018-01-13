using System.Linq;
using KellermanSoftware.CompareNetObjects;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Models.Settings;
using Microsoft.AspNetCore.Http.Features;
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

        private readonly CompareLogic _compareLogic = CreateCompareLogic();
        private readonly Factory<IHttpConnectionFeature> _httpConnectionFeatureFactory;
        private readonly Factory<IHttpRequestFeature> _httpRequestFeatureFactory;


        public SettingsChangesAuditService(Factory<IHttpConnectionFeature> httpConnectionFeatureFactory,
            Factory<IHttpRequestFeature> httpRequestFeatureFactory)
        {
            _httpConnectionFeatureFactory = httpConnectionFeatureFactory;
            _httpRequestFeatureFactory = httpRequestFeatureFactory;
        }

        public SettingsChangesAuditInfo GetChanges(SettingsRoot old, SettingsRoot changed)
        {
            var diff = _compareLogic.Compare(old, changed);
            if (diff.AreEqual)
                return null;

            var diffStr = JsonConvert.SerializeObject(
                diff.Differences.ToDictionary(d => d.PropertyName, d => new[] {d.Object1, d.Object2}),
                JsonSerializerSettings);

            var httpConnectionFeature = _httpConnectionFeatureFactory.Get();
            var httpRequestFeature = _httpRequestFeatureFactory.Get();
            return new SettingsChangesAuditInfo(httpConnectionFeature.RemoteIpAddress,
                httpRequestFeature.Headers["User-Info"].ToDelimitedString(", "), httpRequestFeature.Path, diffStr);
        }

        private static CompareLogic CreateCompareLogic()
        {
            return new CompareLogic(new ComparisonConfig
            {
                MaxDifferences = int.MaxValue,
                AutoClearCache = false,
                MaxByteArrayDifferences = int.MaxValue,
                MaxStructDepth = int.MaxValue,
                ExpectedName = "Old",
                ActualName = "New",
            });
        }
    }
}