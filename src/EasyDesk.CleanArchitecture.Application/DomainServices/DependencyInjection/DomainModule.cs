using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;

public class DomainModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<DomainEventQueue>();
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventQueue>());
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomain(this AppBuilder builder)
    {
        return builder.AddModule(new DomainModule());
    }
}
