using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDomainModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<IPersonRepository, EfCorePersonRepository>();
        services.AddScoped<IPetRepository, EfCorePetRepository>();
    }
}
