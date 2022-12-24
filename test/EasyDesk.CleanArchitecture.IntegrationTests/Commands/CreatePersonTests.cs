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
        var response = await CreatePerson()
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMakeItPossibleToReadThePersonAfterSuccessfullyCreatingOne()
    {
        var person = await CreatePerson().AsDataOnly<PersonDto>();

        var response = await GetPerson(person.Id).AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        var person = await CreatePerson().AsDataOnly<PersonDto>();

        await bus.WaitForMessageOrFail(new PersonCreated(person.Id));
    }

    [Fact]
    public async Task ShouldBeMultitenant()
    {
        var person = await CreatePerson().AsDataOnly<PersonDto>();

        var response = await GetPerson(person.Id)
            .Tenant("other-tenant")
            .AuthenticateAs(AdminId)
            .AsVerifiableErrorResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        var response = await CreatePerson()
            .NoTenant()
            .AsVerifiableErrorResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfAnonymous()
    {
        var response = await CreatePerson()
            .NoAuthentication()
            .AsVerifiableErrorResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldSucceedWithManyWriteRequests()
    {
        for (var i = 0; i < 50; i++)
        {
            await CreatePerson()
                .AsVerifiableResponse<PersonDto>();
        }
    }

    [Fact]
    public async Task ShouldSucceedWithManyReadRequests()
    {
        for (var i = 0; i < 150; i++)
        {
            await Http.GetPeople()
                .AsVerifiableResponse<IEnumerable<PersonDto>>();
        }
    }

    [Fact]
    public async Task ShouldAlsoSendACommandToCreateThePersonsBestFriend()
    {
        var person = await CreatePerson().AsDataOnly<PersonDto>();

        var response = await Http.GetOwnedPets(person.Id).PollUntil<IEnumerable<PetDto>>(r => r.Data.Any());

        await Verify(response);
    }
}
