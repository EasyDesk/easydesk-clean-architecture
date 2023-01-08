using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class IncomingCommandsTests : SampleIntegrationTest
{
    public IncomingCommandsTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-asd";
        var bus = NewBus();
        await bus.Send(new CreateTenant(tenantName));

        await WebService.WaitUntilTenantExists(TenantId.Create(tenantName));
    }

    [Fact]
    public async Task RemoveTenant_ShouldSucceed()
    {
        var tenantName = "test-tenant-qwe";
        var bus = NewBus();
        await bus.Send(new CreateTenant(tenantName));

        var tenantId = TenantId.Create(tenantName);
        await WebService.WaitUntilTenantExists(tenantId);

        await bus.Send(new RemoveTenant(tenantName));

        var tenantChecker = new InjectedServiceCheckBuilder<IMultitenancyManager>(WebService.Services);
        await tenantChecker.WaitUntil(async m => !await m.TenantExists(tenantId));
    }
}
