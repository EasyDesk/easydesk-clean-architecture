﻿using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.DomainEvents;
using EasyDesk.SampleApp.Application.IncomingCommands;
using EasyDesk.SampleApp.Application.OutgoingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePersonTests : SampleIntegrationTest
{
    private const string Tenant = "test-tenant";
    private const string AdminId = "test-admin";

    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2),
        Residence: new("Calvin", "street", "15", "New York", null, null, null, "New York State", "USA"));

    public CreatePersonTests(SampleAppTestsFixture fixture) : base(fixture)
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

    private HttpSingleRequestExecutor<PersonDto> CreatePerson() => Http
        .CreatePerson(_body);

    private HttpSingleRequestExecutor<PersonDto> GetPerson(Guid userId) => Http
        .GetPerson(userId);

    [Fact]
    public async Task ShouldSucceed()
    {
        var response = await CreatePerson()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailWithEmptyAddress()
    {
        var response = await Http
            .CreatePerson(_body with { Residence = new(string.Empty) })
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMakeItPossibleToReadThePersonAfterSuccessfullyCreatingOne()
    {
        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        var person = await CreatePerson()
            .Send()
            .AsData();

        await bus.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var otherTenant = "other-tenant";
        var bus = NewBus();
        await bus.Send(new CreateTenant(otherTenant));
        await WebService.WaitUntilTenantExists(TenantId.Create(otherTenant));

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Tenant(otherTenant)
            .AuthenticateAs(AdminId)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFail_WithInvalidTenant()
    {
        var otherTenant = Enumerable.Repeat("a", TenantId.MaxLength + 1).ConcatStrings();

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Tenant(otherTenant)
            .AuthenticateAs(AdminId)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFail_WithNonExistingTenant()
    {
        var otherTenant = "other-tenant";

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Tenant(otherTenant)
            .AuthenticateAs(AdminId)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        var response = await CreatePerson()
            .NoTenant()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        var response = await CreatePerson()
            .NoAuthentication()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldSucceedWithManyWriteRequests()
    {
        for (var i = 0; i < 150; i++)
        {
            await CreatePerson()
                .Send()
                .EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyPaginatedReadRequests()
    {
        await CreatePerson()
            .Send()
            .EnsureSuccess();
        for (var i = 0; i < 150; i++)
        {
            await Http
                .GetPeople()
                .Send()
                .EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyReadRequests()
    {
        var person = await CreatePerson()
            .Send()
            .AsData();
        for (var i = 0; i < 150; i++)
        {
            await Http
                .GetPerson(person.Id)
                .Send()
                .EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldAlsoSendACommandToCreateThePersonsBestFriend()
    {
        var person = await CreatePerson().Send().AsData();

        var response = await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task CreateManyPeople()
    {
        foreach (var i in Enumerable.Range(0, 150))
        {
            var body = new CreatePersonBodyDto(
                $"test-name-{i}",
                $"test-last-name-{i}",
                new LocalDate(1992, 3, 12).PlusDays(i),
                new("number", StreetNumber: i.ToString()));
            await Http.CreatePerson(body).Send().EnsureSuccess();
        }

        var response = await Http
            .GetPeople()
            .Send()
            .AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldCreateThePersonsPassport()
    {
        var bus = NewBus(OtherServicesEndpoints.PassportService);

        var person = await CreatePerson().Send().AsData();

        await bus.WaitForMessageOrFail(new CreatePassport(
            person.Id,
            person.FirstName,
            person.LastName,
            person.DateOfBirth));
    }

    [Fact]
    public async Task ShouldFailIfUnauthorized()
    {
        var response = await CreatePerson()
            .AuthenticateAs("non-admin-id")
            .Send()
            .AsVerifiable();

        await Verify(response);
    }
}