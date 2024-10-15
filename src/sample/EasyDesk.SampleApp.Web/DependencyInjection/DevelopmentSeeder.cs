using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Seeding;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class DevelopmentSeeder
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DevelopmentSeeder> _logger;

    public DevelopmentSeeder(IServiceProvider services, ILogger<DevelopmentSeeder> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task Seed()
    {
        var admin = Agent.FromSingleIdentity(Realms.MainRealm, IdentityId.FromRandomGuid());
        var tenantId = TenantId.FromRandomGuid();
        var dispatcher = _services.SetupSelfScopedDispatcher(context: new ContextInfo.AuthenticatedRequest(admin), tenantId);
        await dispatcher.Dispatch(new CreateTenant(tenantId));
        await dispatcher.Dispatch(new AddAdmin());
        _logger.LogWarning("Created tenant {tenantId} and admin with id {adminId}", tenantId, admin.MainIdentity().Id);
        _services.LogForgedJwt(admin);
    }
}
