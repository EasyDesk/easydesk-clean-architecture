using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Mediator.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationModule : AppModule
{
    private readonly Action<AuthorizationOptions> _configure;

    public AuthorizationModule(Action<AuthorizationOptions> configure)
    {
        _configure = configure;
    }

    private class AuthorizationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAuthorizer<TRequest> _authorizer;
        private readonly IUserInfoProvider _userInfoProvider;

        public AuthorizationBehaviorWrapper(IAuthorizer<TRequest> authorizer, IUserInfoProvider userInfoProvider)
        {
            _authorizer = authorizer;
            _userInfoProvider = userInfoProvider;
        }

        protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
        {
            var behaviorType = typeof(AuthorizationBehavior<,>).MakeGenericType(requestType, responseType);
            return Activator.CreateInstance(behaviorType, _authorizer, _userInfoProvider) as IPipelineBehavior<TRequest, TResponse>;
        }
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<MediatrModule>().Pipeline.AddBehavior(typeof(AuthorizationBehaviorWrapper<,>));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var options = new AuthorizationOptions(services, app);
        services.AddScoped(typeof(IAuthorizer<>), typeof(NoAuthorizer<>));
        _configure?.Invoke(options);
    }
}

public static class AuthorizationModuleExtensions
{
    public static AppBuilder AddAuthorization(this AppBuilder builder, Action<AuthorizationOptions> configure = null)
    {
        return builder.AddModule(new AuthorizationModule(configure));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasModule<AuthorizationModule>();
}
