using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.IncomingCommands;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Auditing;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class AuditingTests : SampleIntegrationTest
{
    private const string Tenant = "tenant-id";
    private const string AdminId = "admin-id";

    private Guid _personId;
    private int _initialAudits;

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
        _initialAudits++;
        await WebService.WaitUntilTenantExists(TenantId.Create(Tenant));
        await Http.AddAdmin().Send().EnsureSuccess();
        _initialAudits++;

        var createPersonBody = new CreatePersonBodyDto(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: new LocalDate(2012, 12, 21),
            Residence: new(StreetName: "Abbey Road"));
        _personId = await Http.CreatePerson(createPersonBody).Send().AsData().Map(x => x.Id);
        _initialAudits += 2;
        await Http.GetOwnedPets(_personId).PollUntil(pets => pets.Any()).EnsureSuccess();
        await WebService.WaitConditionUnderTenant<IAuditLog>(
            TenantInfo.Public,
            log => log
                .Audit(new())
                .GetAllItems(50)
                .ToEnumerableAsync()
                .Map(e => e.Count() == _initialAudits));
    }

    private Task WaitUntilAuditLogHasMoreRecords(int newRecords) =>
        WebService.WaitConditionUnderTenant<IAuditLog>(
            TenantInfo.Public,
            log => log
                .Audit(new())
                .GetAllItems(50)
                .ToEnumerableAsync()
                .Map(e => e.Count() == _initialAudits + newRecords));

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

    [Fact]
    public async Task ShouldAuditCommandRequests()
    {
        await Http.CreatePet(_personId, new("Bobby")).Send().EnsureSuccess();

        await WaitUntilAuditLogHasMoreRecords(1);

        var response = await Http.GetAudits().Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFilterByUserId()
    {
        var response = await Http
            .GetAudits()
            .WithQuery("userId", AdminId)
            .Send()
            .AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFilterBySuccess()
    {
        var success = await Http.CreatePet(Guid.NewGuid(), new("Bobby")).Send().IsSuccess();
        success.ShouldBeFalse();

        await WaitUntilAuditLogHasMoreRecords(1);

        Task<IEnumerable<AuditRecordDto>> RunQuery(bool success) => Http
            .GetAudits()
            .WithQuery("success", success.ToString())
            .Send()
            .AsVerifiableEnumerable();

        await Verify(new
        {
            Failing = await RunQuery(false),
            Succeeding = await RunQuery(true),
        });
    }

    [Fact]
    public async Task ShouldFilterByAnonymous()
    {
        Task<IEnumerable<AuditRecordDto>> RunQuery(bool anonymous) => Http
            .GetAudits()
            .WithQuery("anonymous", anonymous.ToString())
            .Send()
            .AsVerifiableEnumerable();

        await Verify(new
        {
            Anonymous = await RunQuery(true),
            MadeByAUser = await RunQuery(false),
        });
    }

    [Fact]
    public async Task ShouldFilterByName()
    {
        var response = await Http
            .GetAudits()
            .WithQuery("name", nameof(CreatePerson))
            .Send()
            .AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFilterByType()
    {
        var response = await Http
            .GetAudits()
            .WithQuery("type", AuditRecordType.CommandRequest.ToString())
            .Send()
            .AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFilterByInterval()
    {
        void Advance() => Clock.AdvanceSeconds(1);
        Task RunCommand(string name) => Http.CreatePet(_personId, new(name)).Send().EnsureSuccess();

        await RunCommand("Bobby0");
        Advance();
        var from = Clock.GetCurrentInstant();
        Advance();
        await RunCommand("Bobby1");
        await RunCommand("Bobby2");
        Advance();
        var to = Clock.GetCurrentInstant();
        Advance();
        await RunCommand("Bobby3");

        await WaitUntilAuditLogHasMoreRecords(4);

        var response = await Http
            .GetAudits()
            .WithQuery("from", from.ToString())
            .WithQuery("to", to.ToString())
            .Send()
            .AsVerifiableEnumerable();

        await Verify(response);
    }
}
