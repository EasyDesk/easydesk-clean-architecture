using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Infrastructure.Seeding;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;

namespace EasyDesk.SampleApp.Web.DependencyInjection;

public class DevelopmentSeeder
{
    private readonly DispatcherFactory _dispatcherFactory;
    private readonly JwtLogger _jwtLogger;
    private readonly ILogger<DevelopmentSeeder> _logger;

    public DevelopmentSeeder(
        DispatcherFactory dispatcherFactory,
        JwtLogger jwtLogger,
        ILogger<DevelopmentSeeder> logger)
    {
        _dispatcherFactory = dispatcherFactory;
        _jwtLogger = jwtLogger;
        _logger = logger;
    }

    public async Task Seed()
    {
        var admin = Agent.FromSingleIdentity(Realms.MainRealm, IdentityId.FromRandomGuid());
        var dispatcher = _dispatcherFactory.CreateProgrammaticDispatcher(admin);
        await dispatcher.Dispatch(new AddAdmin());
        _logger.LogWarning("Created admin with id {AdminId}", admin.MainIdentity().Id);
        _jwtLogger.LogForgedJwt(admin);
    }
}
