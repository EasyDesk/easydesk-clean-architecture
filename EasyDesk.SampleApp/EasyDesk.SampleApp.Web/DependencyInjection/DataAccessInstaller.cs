using EasyDesk.CleanArchitecture.Dal.EfCore;
using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.SampleApp.Web.DependencyInjection
{
    public class DataAccessInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddEfCoreDataAccess(configuration.GetConnectionString("SampleDb"), config =>
            {
                config
                    .AddEntities<SampleAppContext>()
                    .AddOutbox()
                    .AddIdemptenceManager();
            });
        }
    }
}
