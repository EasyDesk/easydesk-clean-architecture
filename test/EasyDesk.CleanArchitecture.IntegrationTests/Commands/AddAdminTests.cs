using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class AddAdminTests : SampleIntegrationTest
{
    private const string Tenant = "test-tenant";
    private const string AdminId = "test-admin";

    public AddAdminTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req
        .Tenant(Tenant)
        .AuthenticateAs(AdminId);

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(Tenant));
        await WebService.WaitUntilTenantExists(TenantId.Create(Tenant));
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        await Http.AddAdmin().Send().EnsureSuccess();

        await WebService.WaitConditionUnderTenant<IUserRolesProvider>(
            TenantId.Create(Tenant),
            async p => (await p.GetRolesForUser(new UserInfo(AdminId))).Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant()
    {
        await Http.AddAdmin().NoTenant().Send().EnsureSuccess();

        await WebService.WaitConditionUnderTenant<IUserRolesProvider>(
            TenantId.Create(Tenant),
            async p => (await p.GetRolesForUser(new UserInfo(AdminId))).Contains(Roles.Admin));
    }

    [Fact]
    public async Task ShouldSucceed_WithPublicTenant_Again()
    {
        await Http.AddAdmin().NoTenant().Send().EnsureSuccess();

        await WebService.WaitConditionUnderTenant<IUserRolesProvider>(
            TenantId.Create(Tenant),
            async p => (await p.GetRolesForUser(new UserInfo(AdminId))).Contains(Roles.Admin));
    }
}
