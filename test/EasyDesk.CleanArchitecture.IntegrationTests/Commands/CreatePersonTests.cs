using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Application.OutgoingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePersonTests : SampleIntegrationTest
{
    private const string TenantId = "test-tenant";
    private const string AdminId = "test-admin";

    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2));

    public CreatePersonTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId).AuthenticateAs(AdminId);

    private HttpRequestBuilder CreatePerson() => Http
        .CreatePerson(_body);

    private HttpRequestBuilder GetPerson(Guid userId) => Http
        .GetPerson(userId);

    [Fact]
    public async Task ShouldSucceed()
    {
        var response = await CreatePerson().Single<PersonDto>()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMakeItPossibleToReadThePersonAfterSuccessfullyCreatingOne()
    {
        var person = await CreatePerson().Single<PersonDto>().Send().AsData();

        var response = await GetPerson(person.Id).Single<PersonDto>().Send().AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        var person = await CreatePerson().Single<PersonDto>().Send().AsData();

        await bus.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var person = await CreatePerson().Single<PersonDto>().Send().AsData();

        var response = await GetPerson(person.Id)
            .Tenant("other-tenant")
            .AuthenticateAs(AdminId)
            .Single<PersonDto>()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        var response = await CreatePerson()
            .NoTenant()
            .Single<PersonDto>()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        var response = await CreatePerson()
            .NoAuthentication()
            .Single<PersonDto>()
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldSucceedWithManyWriteRequests()
    {
        for (var i = 0; i < 150; i++)
        {
            await CreatePerson().Single<PersonDto>().Send().EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyPaginatedReadRequests()
    {
        await CreatePerson().Single<PersonDto>().Send().EnsureSuccess();
        for (var i = 0; i < 150; i++)
        {
            await Http.GetPeople().Paginated<PersonDto>().CollectEveryPage().EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyReadRequests()
    {
        var person = await CreatePerson().Single<PersonDto>().Send().AsData();
        for (var i = 0; i < 150; i++)
        {
            await Http.GetPerson(person.Id).Single<PersonDto>().Send().EnsureSuccess();
        }
    }

    /* TODO: add polling to pagination
    [Fact]
    public async Task ShouldAlsoSendACommandToCreateThePersonsBestFriend()
    {
        var person = await CreatePerson().Single<PersonDto>().Send().AsData();

        var response = await Http.GetOwnedPets(person.Id).Paginated<PetDto>().PollUntil<IEnumerable<PetDto>>(r => r.Any());

        await Verify(response);
    }
    */

    [Fact]
    public async Task CreateManyPeople()
    {
        for (int i = 0; i < 150; i++)
        {
            var body = new CreatePersonBodyDto($"test-name-{i}", $"test-last-name-{i}", LocalDate.FromDateTime(DateTime.UnixEpoch.AddDays(i)));
            await Http.CreatePerson(body).Single<PersonDto>().Send().EnsureSuccess();
        }

        var response = await Http.GetPeople().Paginated<PersonDto>().CollectEveryPage().AsVerifiable();

        await Verify(response);
    }
}
