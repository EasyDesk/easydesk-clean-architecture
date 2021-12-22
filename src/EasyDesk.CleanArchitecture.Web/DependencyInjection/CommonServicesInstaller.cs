using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Infrastructure.Environment;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Time;
using EasyDesk.CleanArchitecture.Infrastructure.UserInfo;
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
                .AddTenantManagement()
                .AddNewtonsoftJsonSerializer()
                .AddHttpContextAccessor()
                .AddTimestampProvider(config)
                .AddUserInfo()
                .AddEnvironmentInfo();
        }
    }
}
