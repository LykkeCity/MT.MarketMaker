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
            request.Headers.TryAddWithoutValidation("User-Info", $"{context?.User?.Identity?.Name}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}