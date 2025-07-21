using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class AuditingTests : SampleAppIntegrationTest
{
    private Guid _personId;
    private int _initialAudits;

    public AuditingTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    protected override void ConfigureSession(SessionConfigurer configurer)
    {
        configurer.SetDefaultAgent(TestAgents.Admin);
        configurer.SetDefaultTenant(SampleSeeder.Data.TestTenant);
    }

    protected override async Task OnInitialization()
    {
        _initialAudits = TestData.OperationsRun;

        await Session.Http.AddAdmin().Send().EnsureSuccess();
        _initialAudits++;

        var createPersonBody = new CreatePersonBodyDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new(2012, 12, 21),
            Residence = AddressDto.Create(streetName: "Abbey Road"),
        };
        _personId = await Session.Http.CreatePerson(createPersonBody).Send().AsData().Map(x => x.Id);
        _initialAudits += 2;
        await Session.Http.GetOwnedPets(_personId).PollUntil(pets => pets.Any()).EnsureSuccess();

        using var scope = Session.TenantManager.MoveToPublic();
        await WaitUntilAuditLogHasMoreRecords(0);
    }

    private async Task WaitUntilAuditLogHasMoreRecords(int newRecords)
    {
        using var scope = Session.TenantManager.MoveToPublic();
        await Session.PollServiceUntil<IAuditLog>(
            log => log
                .Audit(new())
                .GetAll()
                .Map(e => e.Count() == _initialAudits + newRecords));
    }

    [Fact]
    public async Task ShouldReturnInitialAudits()
    {
        await Session.Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldAuditCommands()
    {
        var tenantId = new TenantId("new-tenant");
        await Session.DefaultBusEndpoint.Send(new CreateTenant(tenantId.Value));
        await Session.Host.WaitUntilTenantExists(tenantId);

        using var scope = Session.TenantManager.MoveToTenant(tenantId);
        await Session.PollServiceUntil<IAuditLog>(
            log => log.Audit(new AuditQuery()).GetAll().Map(audits => audits.Any()));

        await Session.Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldAuditCommandRequests()
    {
        await Session.Http.CreatePet(_personId, new("Bobby")).Send().EnsureSuccess();

        await WaitUntilAuditLogHasMoreRecords(1);

        await Session.Http.GetAudits().Send().Verify();
    }

    [Fact]
    public async Task ShouldFilterByIdentity()
    {
        await Session.Http
            .GetAudits()
            .With(x => x.Query("identity", TestAgents.Admin.MainIdentity().Id))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFilterBySuccess()
    {
        var success = await Session.Http.CreatePet(Guid.NewGuid(), new("Bobby")).Send().IsSuccess();
        success.ShouldBeFalse();

        await WaitUntilAuditLogHasMoreRecords(1);

        Task<IEnumerable<AuditRecordDto>> RunQuery(bool success) => Session.Http
            .GetAudits()
            .With(x => x.Query("success", success.ToString()))
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
        Task<IEnumerable<AuditRecordDto>> RunQuery(bool anonymous) => Session.Http
            .GetAudits()
            .With(x => x.Query("anonymous", anonymous.ToString()))
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
        await Session.Http
            .GetAudits()
            .With(x => x.Query("name", nameof(CreatePerson)))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFilterByType()
    {
        await Session.Http
            .GetAudits()
            .With(x => x.Query("type", nameof(AuditRecordType.CommandRequest)))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFilterByInterval()
    {
        void Advance() => Session.Clock.AdvanceSeconds(1);
        Task RunCommand(string name) => Session.Http.CreatePet(_personId, new(name)).Send().EnsureSuccess();

        await RunCommand("Bobby0");
        Advance();
        var from = Session.Clock.GetCurrentInstant();
        Advance();
        await RunCommand("Bobby1");
        await RunCommand("Bobby2");
        Advance();
        var to = Session.Clock.GetCurrentInstant();
        Advance();
        await RunCommand("Bobby3");

        await WaitUntilAuditLogHasMoreRecords(4);

        await Session.Http
            .GetAudits()
            .With(x => x.Query("from", from.ToString()))
            .With(x => x.Query("to", to.ToString()))
            .Send()
            .Verify();
    }
}
