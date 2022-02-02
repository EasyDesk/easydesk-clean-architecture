using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Mediator.DependencyInjection;

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
        if (app.HasRebusMessaging())
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionScopeBehavior<,>));
        }
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviorWrapper<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainConstraintsViolationHandlerWrapper<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventHandlingBehaviorWrapper<,>));
    }
}

public static class MediatrModuleExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder)
    {
        return builder.AddModule(new MediatrModule());
    }
}
