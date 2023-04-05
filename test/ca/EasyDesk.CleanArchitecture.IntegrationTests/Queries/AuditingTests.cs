using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class AuditingTests : SampleIntegrationTest
{
    private const string Tenant = "tenant-id";
    private const string AdminId = "admin-id";

    public AuditingTests(SampleAppTestsFixture factory) : base(factory)
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
        await Http.AddAdmin().Send().EnsureSuccess();
    }

    [Fact]
    public async Task ShouldReturnInitialAudits()
    {
        var response = await Http.GetAudits().Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldAuditCommands()
    {
        var bus = NewBus();
        var tenantId = TenantId.Create("new-tenant");
        await bus.Send(new CreateTenant(tenantId.Value));
        await WebService.WaitUntilTenantExists(tenantId);

        await WebService.WaitConditionUnderTenant<IAuditLog>(
            tenantId,
            log => log.Audit(new AuditQuery()).GetAllItems(1).Any());

        var response = await Http.GetAudits().Tenant(tenantId.Value).Send().AsVerifiableEnumerable();

        await Verify(response);
    }
}
