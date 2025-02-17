using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public class DataAccessModule : AppModule
{
    public DataAccessModule(IDataAccessImplementation implementation)
    {
        Implementation = implementation;
    }

    public IDataAccessImplementation Implementation { get; }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline
                .AddStep(typeof(UnitOfWorkStep<,>));
            pipeline
                .AddStepAfterAll(typeof(SaveChangesStep<,>))
                .After(typeof(DomainEventHandlingStep<,>));
            Implementation.ConfigurePipeline(pipeline);
        });
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        Implementation.AddMainDataAccessServices(services, app);
    }
}

public static class DataAccessModuleExtensions
{
    public static IAppBuilder AddDataAccess(this IAppBuilder builder, IDataAccessImplementation implementation)
    {
        return builder.AddModule(new DataAccessModule(implementation));
    }
}
