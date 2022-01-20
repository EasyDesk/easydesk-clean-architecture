using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class MediatrFeature : IAppFeature
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

public static class MediatrFeatureExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder)
    {
        return builder.AddFeature(new MediatrFeature());
    }
}
