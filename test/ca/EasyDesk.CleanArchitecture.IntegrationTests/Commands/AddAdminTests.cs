using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleIntegrationTest
{
    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    private async Task WaitForConditionOnRoles(Func<IFixedSet<Role>, bool> condition)
    {
        await PollServiceUntil<IAgentRolesProvider>(async p => condition(await p.GetRolesForAgent(TestAgents.Admin)));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        using var scope = TenantManager.MoveToTenant(SampleSeeder.Data.TestTenant);
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
        using var scope = TenantManager.MoveToTenant(SampleSeeder.Data.TestTenant);
        await Http.AddAdmin().Send().EnsureSuccess();

        await DefaultBusEndpoint.Send(new RemoveTenant(SampleSeeder.Data.TestTenant));
        await DefaultBusEndpoint.Send(new CreateTenant(SampleSeeder.Data.TestTenant));

        await WaitForConditionOnRoles(r => !r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldNotReset_AfterTenantIsDeleted_WithPublicRole()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        await DefaultBusEndpoint.Send(new RemoveTenant(SampleSeeder.Data.TestTenant));
        await DefaultBusEndpoint.Send(new CreateTenant(SampleSeeder.Data.TestTenant));

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }
}
