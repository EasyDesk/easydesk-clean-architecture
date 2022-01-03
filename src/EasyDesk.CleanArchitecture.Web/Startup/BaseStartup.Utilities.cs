using EasyDesk.CleanArchitecture.Application.Events.DomainEvents;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Infrastructure.Environment;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Time;
using EasyDesk.CleanArchitecture.Web.UserInfo;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    private void AddUtilityDomainServices(IServiceCollection services)
    {
        services
            .AddScoped<IDomainEventNotifier, TransactionalDomainEventQueue>()
            .AddNewtonsoftJsonSerializer(ConfigureJsonSerializerSettings)
            .AddHttpContextAccessor()
            .AddTimestampProvider(Configuration)
            .AddUserInfo()
            .AddEnvironmentInfo();
    }

    protected virtual void ConfigureJsonSerializerSettings(JsonSerializerSettings serializerSettings)
    {
    }
}
