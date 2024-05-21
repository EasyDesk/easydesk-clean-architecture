using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.Authorization;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public abstract class AbstractRemoveAdminTests : SampleIntegrationTest
{
    protected AbstractRemoveAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected abstract HttpSingleRequestExecutor<Nothing> RemoveAdmin();

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    protected override async Task OnInitialization()
    {
        await Http.AddAdmin().Send().EnsureSuccess();
        await WaitForConditionOnRoles(roles => roles.Contains(Roles.Admin));
    }

    private async Task WaitForConditionOnRoles(Func<IImmutableSet<Role>, bool> condition)
    {
        await PollServiceUntil<IAgentRolesProvider>(
            async p => condition(await p.GetRolesForAgent(TestAgents.Admin)));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        await RemoveAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => !r.Contains(Roles.Admin));
    }
}

public class RemoveAdminTests : AbstractRemoveAdminTests
{
    public RemoveAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override HttpSingleRequestExecutor<Nothing> RemoveAdmin() => Http.RemoveAdmin();
}

public class RemoveRolesTests : AbstractRemoveAdminTests
{
    public RemoveRolesTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override HttpSingleRequestExecutor<Nothing> RemoveAdmin() => Http.RemoveAllRoles();
}
