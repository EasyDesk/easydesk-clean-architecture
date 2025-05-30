﻿using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

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

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        Implementation.AddMainDataAccessServices(registry, app);
    }
}

public static class DataAccessModuleExtensions
{
    public static IAppBuilder AddDataAccess(this IAppBuilder builder, IDataAccessImplementation implementation)
    {
        return builder.AddModule(new DataAccessModule(implementation));
    }
}
