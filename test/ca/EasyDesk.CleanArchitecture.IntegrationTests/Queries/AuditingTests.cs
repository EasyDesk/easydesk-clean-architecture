using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class AuditingTests : SampleIntegrationTest
{
    private Guid _personId;
    private int _initialAudits;

    public AuditingTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    protected override async Task OnInitialization()
    {
        _initialAudits = TestData.OperationsRun;

        await Http.AddAdmin().Send().EnsureSuccess();
        _initialAudits++;

        var createPersonBody = new CreatePersonBodyDto(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: new LocalDate(2012, 12, 21),
            Residence: AddressDto.Create(streetName: "Abbey Road"));
        _personId = await Http.CreatePerson(createPersonBody).Send().AsData().Map(x => x.Id);
        _initialAudits += 2;
        await Http.GetOwnedPets(_personId).PollUntil(pets => pets.Any()).EnsureSuccess();

        using var scope = TenantManager.MoveToPublic();
        await WaitUntilAuditLogHasMoreRecords(0);
    }

    private Task WaitUntilAuditLogHasMoreRecords(int newRecords)
    {
        using var scope = TenantManager.MoveToPublic();
        return PollServiceUntil<IAuditLog>(
            log => log
                .Audit(new())
                .GetAll()
                .Map(e => e.Count() == _initialAudits + newRecords));
    }

    [Fact]
    public async Task ShouldReturnInitialAudits()
    {
        await Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldAuditCommands()
    {
        var tenantId = new TenantId("new-tenant");
        await DefaultBusEndpoint.Send(new CreateTenant(tenantId.Value));
        await WebService.WaitUntilTenantExists(tenantId);

        using var scope = TenantManager.MoveToTenant(tenantId);
        await PollServiceUntil<IAuditLog>(
            log => log.Audit(new AuditQuery()).GetAll().Map(audits => audits.Any()));

        await Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldAuditCommandRequests()
    {
        await Http.CreatePet(_personId, new("Bobby")).Send().EnsureSuccess();

        await WaitUntilAuditLogHasMoreRecords(1);

        await Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldFilterByIdentity()
    {
        await Http
            .GetAudits()
            .WithQuery("identity", TestAgents.Admin.MainIdentity().Id)
            .Send()
            .Verify();
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
            MadeBySomeone = await RunQuery(false),
        });
    }

    [Fact]
    public async Task ShouldFilterByName()
    {
        await Http
            .GetAudits()
            .WithQuery("name", nameof(CreatePerson))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFilterByType()
    {
        await Http
            .GetAudits()
            .WithQuery("type", AuditRecordType.CommandRequest.ToString())
            .Send()
            .Verify();
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

        await Http
            .GetAudits()
            .WithQuery("from", from.ToString())
            .WithQuery("to", to.ToString())
            .Send()
            .Verify();
    }
}
