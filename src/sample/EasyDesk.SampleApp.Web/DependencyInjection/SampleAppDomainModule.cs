using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore.Repositories;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class SampleAppDomainModule : AppModule
{
    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<EfCorePersonRepository>()
            .As<IPersonRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<EfCorePetRepository>()
            .As<IPetRepository>()
            .InstancePerLifetimeScope();
    }
}
