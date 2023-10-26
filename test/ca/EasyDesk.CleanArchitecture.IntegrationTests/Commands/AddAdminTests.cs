using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleIntegrationTest
{
    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Task OnInitialization()
    {
        AuthenticateAs(TestAgents.Admin);
        return Task.CompletedTask;
    }

    private async Task WaitForConditionOnRoles(Func<IImmutableSet<Role>, bool> condition)
    {
        await WebService.WaitConditionUnderTenant<IAgentRolesProvider>(
            SampleSeeder.Data.TestTenant,
            async p => condition(await p.GetRolesForAgent(TestAgents.Admin)));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        TenantNavigator.MoveToTenant(SampleSeeder.Data.TestTenant);
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
        TenantNavigator.MoveToTenant(SampleSeeder.Data.TestTenant);
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
