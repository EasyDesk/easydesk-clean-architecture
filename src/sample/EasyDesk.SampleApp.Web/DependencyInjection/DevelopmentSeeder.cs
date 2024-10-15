using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.AutoScopingDispatchers;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class DevelopmentSeeder
{
    private readonly AutoScopingDispatcherFactory _dispatcherFactory;
    private readonly JwtLogger _jwtLogger;
    private readonly ILogger<DevelopmentSeeder> _logger;

    public DevelopmentSeeder(AutoScopingDispatcherFactory dispatcherFactory, JwtLogger jwtLogger, ILogger<DevelopmentSeeder> logger)
    {
        _dispatcherFactory = dispatcherFactory;
        _jwtLogger = jwtLogger;
        _logger = logger;
    }

    public async Task Seed()
    {
        var admin = Agent.FromSingleIdentity(Realms.MainRealm, IdentityId.FromRandomGuid());
        var tenantId = TenantId.FromRandomGuid();
        var dispatcher = _dispatcherFactory.CreateAutoScopingDispatcher(context: new ContextInfo.AuthenticatedRequest(admin), tenantId);
        await dispatcher.Dispatch(new CreateTenant(tenantId));
        await dispatcher.Dispatch(new AddAdmin());
        _logger.LogWarning("Created tenant {tenantId} and admin with id {adminId}", tenantId, admin.MainIdentity().Id);
        _jwtLogger.LogForgedJwt(admin);
    }
}
