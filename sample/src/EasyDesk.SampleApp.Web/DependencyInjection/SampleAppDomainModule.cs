using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDomainModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<IPersonRepository, EfCorePersonRepository>();
    }
}
