using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public sealed class AuthorizationOptions
{
    public AuthorizationOptions(IServiceCollection services, AppDescription app)
    {
        Services = services;
        App = app;
    }

    public IServiceCollection Services { get; }

    public AppDescription App { get; }
}
