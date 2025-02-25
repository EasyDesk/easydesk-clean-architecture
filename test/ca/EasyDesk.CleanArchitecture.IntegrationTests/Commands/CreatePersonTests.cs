using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
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

public class CreatePersonTests : SampleIntegrationTest
{
    private readonly CreatePersonBodyDto _body = new()
    {
        FirstName = "Foo",
        LastName = "Bar",
        DateOfBirth = new LocalDate(1996, 2, 2),
        Residence = AddressDto.Create("Calvin", "street", "15", "Brooklyn", "New York", null, null, "New York State", "USA"),
    };

    public CreatePersonTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override Option<Agent> DefaultAgent => Some(TestAgents.Admin);

    protected override async Task OnInitialization()
    {
        await Http.AddAdmin().Send().EnsureSuccess();
    }

    private HttpSingleRequestExecutor<PersonDto> CreatePerson() => Http
        .CreatePerson(_body);

    private HttpSingleRequestExecutor<IEnumerable<PersonDto>> CreatePeople(params CreatePersonBodyDto[] extra) => Http
        .CreatePeople(List(
            _body,
            new()
            {
                FirstName = "Baz",
                LastName = "Qux",
                DateOfBirth = new LocalDate(1996, 2, 2),
                Residence = _body.Residence,
            },
            new()
            {
                FirstName = "Asd",
                LastName = "Qwerty",
                DateOfBirth = new LocalDate(1997, 11, 11),
                Residence = _body.Residence,
            }).Concat(extra));

    private HttpSingleRequestExecutor<PersonDto> GetPerson(Guid id) => Http.GetPerson(id);

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
        await Http
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
        await DefaultBusEndpoint.Subscribe<PersonCreated>();

        var person = await CreatePerson()
            .Send()
            .AsData();

        using var scope = TenantManager.Ignore();

        await DefaultBusEndpoint.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldEmitAnEventUnderSpecificTenant()
    {
        await DefaultBusEndpoint.Subscribe<PersonCreated>();

        var person = await CreatePerson()
            .Send()
            .AsData();

        using var scope = TenantManager.MoveToTenant(PersonCreated.EmittedWithTenant);

        await DefaultBusEndpoint.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var otherTenant = new TenantId("other-tenant");
        await DefaultBusEndpoint.Send(new CreateTenant(otherTenant));
        await WebService.WaitUntilTenantExists(otherTenant);

        var person = await CreatePerson()
            .Send()
            .AsData();

        await GetPerson(person.Id)
            .Tenant(otherTenant)
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
            .Headers(h => h.Replace(CommonTenantReaders.TenantIdHttpHeader, otherTenant))
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
            .Tenant(otherTenant)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        await CreatePerson()
            .NoTenant()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        await CreatePerson()
            .NoAuthentication()
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

        await Http
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
            await Http.CreatePerson(body).Send().EnsureSuccess();
        }

        await Http
            .GetPeople()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldCreateThePersonsPassport()
    {
        var bus = NewBusEndpoint(OtherServicesEndpoints.PassportService);

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
            .AuthenticateAs(TestAgents.OtherUser)
            .Send()
            .Verify();
    }

    public static TheoryData<AddressDto, string, string, string> WrongAddresses()
    {
        var data = new TheoryData<AddressDto, string, string, string>();
        var badPlaceNames = new[] { (string.Empty, "empty"), (new string('a', PlaceName.MaxLength + 1), "too long") };
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
        await Http.CreatePerson(body)
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
        await CreatePeople(new CreatePersonBodyDto()
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
            new CreatePersonBodyDto()
            {
                FirstName = "Mario",
                LastName = "Facher",
                DateOfBirth = Clock.GetCurrentInstant().InUtc().Date.PlusYears(1),
                Residence = _body.Residence,
            },
            new CreatePersonBodyDto()
            {
                FirstName = "Mario",
                LastName = "Facher",
                DateOfBirth = Clock.GetCurrentInstant().InUtc().Date.PlusYears(-1),
                Residence = _body.Residence,
            })
            .Send()
            .Verify();

        var people = await Http.GetPeople().Send().AsVerifiableEnumerable();
        people.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldSucceed_CreatePeople_WithInvalidEntry_CausingErrorInHandler_IfEntryErrorIsIgnored()
    {
        await Http.CreatePeople(
            [
                new CreatePersonBodyDto()
                {
                    FirstName = "Mario",
                    LastName = "Facher",
                    DateOfBirth = new LocalDate(1992, 3, 12),
                    Residence = _body.Residence,
                },
                new CreatePersonBodyDto()
                {
                    FirstName = "skip",
                    LastName = "asd",
                    DateOfBirth = new LocalDate(1992, 3, 12),
                    Residence = _body.Residence,
                },
                new CreatePersonBodyDto()
                {
                    FirstName = "Mario",
                    LastName = "Facher",
                    DateOfBirth = new LocalDate(1992, 3, 12),
                    Residence = _body.Residence,
                }
            ])
            .Send()
            .EnsureSuccess();

        await Http
            .GetPeople()
            .Send()
            .Verify();
    }
}
