using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Application.OutgoingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
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
        var response = await CreatePerson().Build()
            .Send()
            .AsVerifiable<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMakeItPossibleToReadThePersonAfterSuccessfullyCreatingOne()
    {
        var person = await CreatePerson().Build().Send().AsData<PersonDto>();

        var response = await GetPerson(person.Id).Build().Send().AsVerifiable<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        var person = await CreatePerson().Build().Send().AsData<PersonDto>();

        await bus.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var person = await CreatePerson().Build().Send().AsData<PersonDto>();

        var response = await GetPerson(person.Id)
            .Tenant("other-tenant")
            .AuthenticateAs(AdminId)
            .Build()
            .Send()
            .AsVerifiable<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        var response = await CreatePerson()
            .NoTenant()
            .Build()
            .Send()
            .AsVerifiable<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        var response = await CreatePerson()
            .NoAuthentication()
            .Build()
            .Send()
            .AsVerifiable<PersonDto>();

        await Verify(response);
    }

    /* TODO: uncomment
    [Fact]
    public async Task ShouldSucceedWithManyWriteRequests()
    {
        for (var i = 0; i < 50; i++)
        {
            await CreatePerson().Send().EnsureSuccess();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyReadRequests()
    {
        for (var i = 0; i < 150; i++)
        {
            await Http.GetPeople().Send().EnsureSuccess();
        }
    }
    */

    [Fact]
    public async Task ShouldAlsoSendACommandToCreateThePersonsBestFriend()
    {
        var person = await CreatePerson().Build().Send().AsData<PersonDto>();

        var response = await Http.GetOwnedPets(person.Id).Build().PollUntil<IEnumerable<PetDto>>(r => r.Any());

        await Verify(response);
    }

    [Fact]
    public async Task CreateManyPeople()
    {
        var list = new List<CreatePersonBodyDto>();
        for (int i = 0; i < 5; i++)
        {
            var body = new CreatePersonBodyDto($"test-name-{i}", $"test-last-name-{i}", LocalDate.FromDateTime(DateTime.UnixEpoch.AddDays(i)));
            list.Add(body);
            await Http.CreatePerson(body).Build().Send();
        }

        var response = await Http.GetPeople().Build().SendForEveryPage().AsVerifiable<PersonDto>();

        await Verify(response);
    }
}
