using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.SampleApp.Web.DependencyInjection
{
    public class SwaggerInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddSwagger("EasyDesk.SampleApp", typeof(Startup));
        }
    }
}
