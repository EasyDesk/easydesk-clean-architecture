using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Environment;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Time;
using EasyDesk.CleanArchitecture.Infrastructure.UserInfo;
using EasyDesk.CleanArchitecture.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public class CommonServicesInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services
                .AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>()
                .AddHttpContextAccessor()
                .AddDateTimeProvider(config)
                .AddDateTimeLocale(config)
                .AddTokenInfoRetriever()
                .AddEnvironmentInfo()
                .AddConfigAsSingleton<DisabledEndpoints>(config);
        }
    }
}
