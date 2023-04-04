using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Auditing.DependencyInjection;

public class AuditingModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStep(typeof(AuditingStep<,>))
            .After(typeof(UnitOfWorkStep<,>))
            .After(typeof(MultitenancyManagementStep<,>)));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddAuditing(services, app);
    }
}

public static class AuditingModuleExtensions
{
    public static AppBuilder AddAuditing(this AppBuilder builder)
    {
        return builder.AddModule(new AuditingModule());
    }
}
