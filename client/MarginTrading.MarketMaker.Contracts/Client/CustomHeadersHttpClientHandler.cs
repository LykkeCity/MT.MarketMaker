using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MarginTrading.MarketMaker.Contracts.Client
{
    internal class CustomHeadersHttpClientHandler : HttpClientHandler
    {
        private readonly string _userAgent;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomHeadersHttpClientHandler(string userAgent, IHttpContextAccessor httpContextAccessor)
        {
            _userAgent = userAgent;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            var context = _httpContextAccessor.HttpContext;
            request.Headers.TryAddWithoutValidation("User-Info", $"{context?.User?.Identity?.Name} ({GetIp(context)})");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static string GetIp([CanBeNull] HttpContext context)
        {
            if (context == null) return "";
            var ip = string.Empty;

            // http://stackoverflow.com/a/43554000/538763
            var xForwardedForVal = GetHeaderValueAs<string>(context, "X-Forwarded-For")
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (!string.IsNullOrEmpty(xForwardedForVal))
                ip = xForwardedForVal.Split(':')[0];

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && context.Connection?.RemoteIpAddress != null)
                ip = context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");

            return ip;
        }
        
        private static T GetHeaderValueAs<T>(HttpContext httpContext, string headerName)
        {
            if (httpContext?.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
            {
                var rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            
            return default(T);
        }
    }
}