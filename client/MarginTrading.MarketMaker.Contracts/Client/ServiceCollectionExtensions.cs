using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.MarketMaker.Contracts.Client
{
    public static class ServiceCollectionExtensions
    {
        [PublicAPI]
        public static void RegisterMtMarketMakerClient(this IServiceCollection services, string url, string userAgent)
        {
            services.AddSingleton<IMtMarketMakerClient>(p => new MtMarketMakerClient(url, userAgent));
        }
    }
}