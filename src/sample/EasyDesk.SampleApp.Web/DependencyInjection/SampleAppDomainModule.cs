using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore.Repositories;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDomainModule : AppModule
{
    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        services.AddScoped<IPersonRepository, EfCorePersonRepository>();
        services.AddScoped<IPetRepository, EfCorePetRepository>();
    }
}
