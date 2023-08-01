using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.DomainEvents;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePersonTests : SampleIntegrationTest
{
    private static readonly TenantId _tenant = TenantId.New("test-tenant");

    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2),
        Residence: AddressDto.Create("Calvin", "street", "15", "Brooklyn", "New York", null, null, "New York State", "USA"));

    public CreatePersonTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(_tenant));
        await WebService.WaitUntilTenantExists(_tenant);

        MoveToTenant(_tenant);
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();
    }

    private HttpSingleRequestExecutor<PersonDto> CreatePerson() => Http
        .CreatePerson(_body);

    private HttpSingleRequestExecutor<PersonDto> GetPerson(Guid id) => Http
        .GetPerson(id);

    [Fact]
    public async Task ShouldSucceed()
    {
        var response = await CreatePerson()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldBeApprovedLater()
    {
        var person = await CreatePerson()
            .Send()
            .AsData();
        person.Approved.ShouldBeFalse();

        var response = await GetPerson(person.Id)
            .PollUntil(p => p.Approved, interval: Duration.FromSeconds(1))
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailWithEmptyAddress()
    {
        var response = await Http
            .CreatePerson(_body with { Residence = AddressDto.Create(string.Empty) })
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
        var otherTenant = TenantId.New("other-tenant");
        var bus = NewBus();
        await bus.Send(new CreateTenant(otherTenant));
        await WebService.WaitUntilTenantExists(otherTenant);

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Tenant(otherTenant)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFail_WithInvalidTenant()
    {
        var otherTenant = new string('a', TenantId.MaxLength + 1);

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Headers(h => h.Replace(CommonTenantReaders.TenantIdHttpHeader, otherTenant))
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFail_WithNonExistingTenant()
    {
        var otherTenant = TenantId.New("other-tenant");

        var person = await CreatePerson()
            .Send()
            .AsData();

        var response = await GetPerson(person.Id)
            .Tenant(otherTenant)
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
                AddressDto.Create("number", streetNumber: i.ToString()));
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
            .AuthenticateAs(TestAgents.OtherUser)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    public static IEnumerable<object[]> WrongAddresses()
    {
        var badPlaceNames = new[] { (string.Empty, "empty"), (new string('a', PlaceName.MaxLength + 1), "too long") };
        foreach (var (streetName, streetNameProblem) in badPlaceNames)
        {
            foreach (var (streetType, streetTypeProblem) in badPlaceNames)
            {
                foreach (var (streetNumber, streetNumberProblem) in badPlaceNames)
                {
                    yield return new object[] { AddressDto.Create(streetName, streetType: streetType, streetNumber: streetNumber), streetNameProblem, streetTypeProblem, streetNumberProblem };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(WrongAddresses))]
    public async Task ShouldFail_WithInvalidResidenceAddresses(AddressDto addressDto, string streetNameProblem, string streetTypeProblem, string streetNumberProblem)
    {
        var body = _body with { Residence = addressDto };
        var response = await Http.CreatePerson(body)
            .Send()
            .AsVerifiable();

        await Verify(response)
            .UseParameters(null, streetNameProblem, streetTypeProblem, streetNumberProblem);
    }
}
