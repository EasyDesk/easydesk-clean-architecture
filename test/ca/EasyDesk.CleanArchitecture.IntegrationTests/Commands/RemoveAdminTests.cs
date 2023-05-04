using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.IncomingCommands;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public abstract class AbstractRemoveAdminTests : SampleIntegrationTest
{
    private static readonly TenantId _tenantId = TenantId.New("test-tenant-kjd");
    private static readonly UserId _adminId = UserId.New("test-admin-asd");

    protected AbstractRemoveAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected abstract HttpSingleRequestExecutor<Nothing> RemoveAdmin();

    protected override void ConfigureRequests(HttpRequestBuilder req) => req
        .Tenant(_tenantId)
        .AuthenticateAs(_adminId);

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(_tenantId));
        await WebService.WaitUntilTenantExists(_tenantId);
        await Http.AddAdmin().Send().EnsureSuccess();
        await WaitForConditionOnRoles(roles => roles.Contains(Roles.Admin));
    }

    private async Task WaitForConditionOnRoles(Func<IImmutableSet<Role>, bool> condition)
    {
        await WebService.WaitConditionUnderTenant<IUserRolesProvider>(
            _tenantId,
            async p => condition(await p.GetRolesForUser(new(_adminId))));
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
