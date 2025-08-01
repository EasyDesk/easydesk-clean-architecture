﻿using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.DomainEvents;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePersonTests : SampleAppIntegrationTest
{
    private readonly CreatePersonBodyDto _body = new()
    {
        FirstName = "Foo",
        LastName = "Bar",
        DateOfBirth = new(1996, 2, 2),
        Residence = AddressDto.Create("Calvin", "street", "15", "Brooklyn", "New York", null, null, "New York State", "USA"),
    };

    public CreatePersonTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureSession(SessionConfigurer configurer)
    {
        configurer.SetDefaultAgent(TestAgents.Admin);
        configurer.SetDefaultTenant(SampleSeeder.Data.TestTenant);
    }

    protected override async Task OnInitialization()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();
    }

    private HttpSingleRequestExecutor<PersonDto> CreatePerson() => Session.Http
        .CreatePerson(_body);

    private HttpSingleRequestExecutor<IEnumerable<PersonDto>> CreatePeople(params CreatePersonBodyDto[] extra) => Session.Http
        .CreatePeople(
            List(
                _body,
                new()
                {
                    FirstName = "Baz",
                    LastName = "Qux",
                    DateOfBirth = new(1996, 2, 2),
                    Residence = _body.Residence,
                },
                new()
                {
                    FirstName = "Asd",
                    LastName = "Qwerty",
                    DateOfBirth = new(1997, 11, 11),
                    Residence = _body.Residence,
                })
                .Concat(extra));

    private HttpSingleRequestExecutor<PersonDto> GetPerson(Guid id) => Session.Http.GetPerson(id);

    [Fact]
    public async Task ShouldSucceed()
    {
        await CreatePerson()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldBeApprovedLater()
    {
        var person = await CreatePerson()
            .Send()
            .AsData();
        person.Approved.ShouldBeFalse();

        await GetPerson(person.Id)
            .PollUntil(p => p.Approved, interval: Duration.FromSeconds(1))
            .Verify();
    }

    [Fact]
    public async Task ShouldFailWithEmptyAddress()
    {
        await Session.Http
            .CreatePerson(_body with { Residence = AddressDto.Create(string.Empty) })
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMakeItPossibleToReadThePersonAfterSuccessfullyCreatingOne()
    {
        var person = await CreatePerson()
            .Send()
            .AsData();

        await GetPerson(person.Id)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await Session.DefaultBusEndpoint.Subscribe<PersonCreated>();

        var person = await CreatePerson()
            .Send()
            .AsData();

        using var scope = Session.TenantManager.Ignore();

        await Session.DefaultBusEndpoint.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldEmitAnEventUnderSpecificTenant()
    {
        await Session.DefaultBusEndpoint.Subscribe<PersonCreated>();

        var person = await CreatePerson()
            .Send()
            .AsData();

        using var scope = Session.TenantManager.MoveToTenant(PersonCreated.EmittedWithTenant);

        await Session.DefaultBusEndpoint.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var otherTenant = new TenantId("other-tenant");
        await Session.DefaultBusEndpoint.Send(new CreateTenant(otherTenant));
        await Session.Host.WaitUntilTenantExists(otherTenant);

        var person = await CreatePerson()
            .Send()
            .AsData();

        await GetPerson(person.Id)
            .With(x => x.Tenant(otherTenant))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFail_WithInvalidTenant()
    {
        var otherTenant = new string('a', TenantId.MaxLength + 1);

        var person = await CreatePerson()
            .Send()
            .AsData();

        await GetPerson(person.Id)
            .With(h => h.Header(CommonTenantReaders.TenantIdHttpHeader, otherTenant))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFail_WithNonExistingTenant()
    {
        var otherTenant = new TenantId("other-tenant");

        var person = await CreatePerson()
            .Send()
            .AsData();

        await GetPerson(person.Id)
            .With(x => x.Tenant(otherTenant))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        await CreatePerson()
            .With(x => x.NoTenant())
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        await CreatePerson()
            .With(x => x.NoAuthentication())
            .Send()
            .Verify();
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
            await Session.Http
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
            await Session.Http
                .GetPerson(person.Id)
                .Send()
                .EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldAlsoSendACommandToCreateThePersonsBestFriend()
    {
        var person = await CreatePerson().Send().AsData();

        await Session.Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .Verify();
    }

    [Fact]
    public async Task CreateManyPeople()
    {
        foreach (var i in Enumerable.Range(0, 150))
        {
            var body = new CreatePersonBodyDto
            {
                FirstName = $"test-name-{i}",
                LastName = $"test-last-name-{i}",
                DateOfBirth = new LocalDate(1992, 3, 12).PlusDays(i),
                Residence = AddressDto.Create("number", streetNumber: i.ToString()),
            };
            await Session.Http.CreatePerson(body).Send().EnsureSuccess();
        }

        await Session.Http
            .GetPeople()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldCreateThePersonsPassport()
    {
        var bus = Session.NewBusEndpoint(OtherServicesEndpoints.PassportService);

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
        await CreatePerson()
            .With(x => x.AuthenticateAs(TestAgents.OtherUser))
            .Send()
            .Verify();
    }

    public static TheoryData<AddressDto, string, string, string> WrongAddresses()
    {
        var data = new TheoryData<AddressDto, string, string, string>();
        var badPlaceNames = new[] { (string.Empty, "empty"), (new string('a', PlaceName.MaxLength + 1), "too long"), };
        foreach (var (streetName, streetNameProblem) in badPlaceNames)
        {
            foreach (var (streetType, streetTypeProblem) in badPlaceNames)
            {
                foreach (var (streetNumber, streetNumberProblem) in badPlaceNames)
                {
                    data.Add(AddressDto.Create(streetName, streetType: streetType, streetNumber: streetNumber), streetNameProblem, streetTypeProblem, streetNumberProblem);
                }
            }
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(WrongAddresses))]
    public async Task ShouldFail_WithInvalidResidenceAddresses(AddressDto addressDto, string streetNameProblem, string streetTypeProblem, string streetNumberProblem)
    {
        var body = _body with { Residence = addressDto };
        await Session.Http.CreatePerson(body)
            .Send()
            .Verify(x => x.UseParameters(null, streetNameProblem, streetTypeProblem, streetNumberProblem));
    }

    [Fact]
    public async Task ShouldSucced_CreatePeople()
    {
        await CreatePeople()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFail_CreatePeople_WithInvalidEntry()
    {
        await CreatePeople(new CreatePersonBodyDto
        {
            FirstName = "   ",
            LastName = "   ",
            DateOfBirth = _body.DateOfBirth,
            Residence = _body.Residence,
        })
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFail_CreatePeople_WithInvalidEntry_CausingErrorInHandler()
    {
        await CreatePeople(
            new CreatePersonBodyDto
            {
                FirstName = "Mario",
                LastName = "Facher",
                DateOfBirth = Session.Clock.GetCurrentInstant().InUtc().Date.PlusYears(1),
                Residence = _body.Residence,
            },
            new CreatePersonBodyDto
            {
                FirstName = "Mario",
                LastName = "Facher",
                DateOfBirth = Session.Clock.GetCurrentInstant().InUtc().Date.PlusYears(-1),
                Residence = _body.Residence,
            })
            .Send()
            .Verify();

        var people = await Session.Http.GetPeople().Send().AsVerifiableEnumerable();
        people.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldSucceed_CreatePeople_WithInvalidEntry_CausingErrorInHandler_IfEntryErrorIsIgnored()
    {
        await Session.Http.CreatePeople(
            [
                new CreatePersonBodyDto
                {
                    FirstName = "Mario",
                    LastName = "Facher",
                    DateOfBirth = new(1992, 3, 12),
                    Residence = _body.Residence,
                },
                new CreatePersonBodyDto
                {
                    FirstName = "skip",
                    LastName = "asd",
                    DateOfBirth = new(1992, 3, 12),
                    Residence = _body.Residence,
                },
                new CreatePersonBodyDto
                {
                    FirstName = "Mario",
                    LastName = "Facher",
                    DateOfBirth = new(1992, 3, 12),
                    Residence = _body.Residence,
                }
            ])
            .Send()
            .EnsureSuccess();

        await Session.Http
            .GetPeople()
            .Send()
            .Verify();
    }
}
