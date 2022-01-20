using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public class MediatrModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddMediatR(app.ApplicationAssemblyMarker, app.InfrastructureAssemblyMarker);

        if (app.HasRequestValidation())
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorWrapper<,>));
        }
        if (app.HasAuthorization())
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviorWrapper<,>));
        }
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainConstraintsViolationHandlerWrapper<,>));
    }
}

public static class MediatrModuleExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder)
    {
        return builder.AddModule(new MediatrModule());
    }
}
