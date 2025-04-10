using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleAppIntegrationTest
{
    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureSession(SessionConfigurer configurer)
    {
        configurer.SetDefaultAgent(TestAgents.Admin);
    }

    private async Task WaitForConditionOnRoles(Func<IFixedSet<Role>, bool> condition)
    {
        await Session.PollServiceUntil<IAgentRolesProvider>(async p => condition(await p.GetRolesForAgent(TestAgents.Admin)));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        using var scope = Session.TenantManager.MoveToTenant(SampleSeeder.Data.TestTenant);
        await Session.Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant_Again()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();
        await Session.Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldReset_AfterTenantIsDeleted()
    {
        using var scope = Session.TenantManager.MoveToTenant(SampleSeeder.Data.TestTenant);
        await Session.Http.AddAdmin().Send().EnsureSuccess();

        await Session.DefaultBusEndpoint.Send(new RemoveTenant(SampleSeeder.Data.TestTenant));
        await Session.DefaultBusEndpoint.Send(new CreateTenant(SampleSeeder.Data.TestTenant));

        await WaitForConditionOnRoles(r => !r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldNotReset_AfterTenantIsDeleted_WithPublicRole()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();

        await Session.DefaultBusEndpoint.Send(new RemoveTenant(SampleSeeder.Data.TestTenant));
        await Session.DefaultBusEndpoint.Send(new CreateTenant(SampleSeeder.Data.TestTenant));

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }
}
