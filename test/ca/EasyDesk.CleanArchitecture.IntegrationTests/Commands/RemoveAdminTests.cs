using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public abstract class AbstractRemoveAdminTests : SampleAppIntegrationTest
{
    protected AbstractRemoveAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureSession(SessionConfigurer configurer)
    {
        configurer.SetDefaultAgent(TestAgents.Admin);
        configurer.SetDefaultTenant(SampleSeeder.Data.TestTenant);
    }

    protected abstract HttpSingleRequestExecutor<Nothing> RemoveAdmin();

    protected override async Task OnInitialization()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();
        await WaitForConditionOnRoles(roles => roles.Contains(Roles.Admin));
    }

    private async Task WaitForConditionOnRoles(Func<IFixedSet<Role>, bool> condition)
    {
        await Session.PollServiceUntil<IAgentRolesProvider>(
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

    protected override HttpSingleRequestExecutor<Nothing> RemoveAdmin() => Session.Http.RemoveAdmin();
}

public class RemoveRolesTests : AbstractRemoveAdminTests
{
    public RemoveRolesTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override HttpSingleRequestExecutor<Nothing> RemoveAdmin() => Session.Http.RemoveAllRoles();
}
