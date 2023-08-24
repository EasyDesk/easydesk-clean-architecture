using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleIntegrationTest
{
    private static readonly TenantId _tenantId = TenantId.New("test-tenant");

    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(_tenantId));
        await WebService.WaitUntilTenantExists(_tenantId);

        AuthenticateAs(TestAgents.Admin);
    }

    private async Task WaitForConditionOnRoles(Func<IImmutableSet<Role>, bool> condition)
    {
        await WebService.WaitConditionUnderTenant<IAgentRolesProvider>(
            _tenantId,
            async p => condition(await p.GetRolesForAgent(TestAgents.Admin)));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        TenantNavigator.MoveToTenant(_tenantId);
        await Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant_Again()
    {
        await Http.AddAdmin().Send().EnsureSuccess();
        await Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldReset_AfterTenantIsDeleted()
    {
        TenantNavigator.MoveToTenant(_tenantId);
        await Http.AddAdmin().Send().EnsureSuccess();

        var bus = NewBus();
        await bus.Send(new RemoveTenant(_tenantId));
        await bus.Send(new CreateTenant(_tenantId));

        await WaitForConditionOnRoles(r => !r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldNotReset_AfterTenantIsDeleted_WithPublicRole()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        var bus = NewBus();
        await bus.Send(new RemoveTenant(_tenantId));
        await bus.Send(new CreateTenant(_tenantId));

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }
}
