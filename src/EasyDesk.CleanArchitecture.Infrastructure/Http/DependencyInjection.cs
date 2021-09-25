using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Infrastructure.Http
{
    public static class DependencyInjection
    {
        public static IHttpClientBuilder AddHttpServiceFromConfiguration(this IServiceCollection services, IConfiguration configuration, string name)
        {
            var settings = configuration
                .GetSection($"HttpClients:{name}")
                .Get<HttpServiceSettings>();

            return services.AddHttpClient(name, client =>
            {
                if (settings.ApiKey != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", settings.ApiKey);
                }

                if (settings.BaseAddress != null)
                {
                    client.BaseAddress = new Uri(settings.BaseAddress);
                }
            });
        }
    }
}
