using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.IncomingCommands;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleIntegrationTest
{
    private static readonly TenantId _tenantId = TenantId.New("test-tenant");
    private static readonly UserId _adminId = UserId.New("test-admin");

    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req
        .Tenant(_tenantId)
        .AuthenticateAs(_adminId);

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(_tenantId));
        await WebService.WaitUntilTenantExists(_tenantId);
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
        await Http.AddAdmin().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant()
    {
        await Http.AddAdmin().NoTenant().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant_Again()
    {
        await Http.AddAdmin().NoTenant().Send().EnsureSuccess();

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldReset_AfterTenantIsDeleted()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        var bus = NewBus();
        await bus.Send(new RemoveTenant(_tenantId));
        await bus.Send(new CreateTenant(_tenantId));

        await WaitForConditionOnRoles(r => !r.Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldNotReset_AfterTenantIsDeleted_WithPublicRole()
    {
        await Http.AddAdmin().NoTenant().Send().EnsureSuccess();

        var bus = NewBus();
        await bus.Send(new RemoveTenant(_tenantId));
        await bus.Send(new CreateTenant(_tenantId));

        await WaitForConditionOnRoles(r => r.Contains(Roles.Admin));
    }
}
