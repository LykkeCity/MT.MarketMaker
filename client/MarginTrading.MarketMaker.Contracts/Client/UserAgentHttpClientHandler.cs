using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MarginTrading.MarketMaker.Contracts.Client
{
    internal class UserAgentHttpClientHandler : HttpClientHandler
    {
        private readonly string _userAgent;

        public UserAgentHttpClientHandler(string userAgent)
        {
            _userAgent = userAgent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}