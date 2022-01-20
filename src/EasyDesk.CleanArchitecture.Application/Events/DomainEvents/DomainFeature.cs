using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Events.DomainEvents;

public class DomainFeature : IAppFeature
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<IDomainEventNotifier, TransactionalDomainEventQueue>();
    }
}

public static class DomainFeatureExtensions
{
    public static AppBuilder AddDomain(this AppBuilder builder)
    {
        return builder.AddFeature(new DomainFeature());
    }
}
