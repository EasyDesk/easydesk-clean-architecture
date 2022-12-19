using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.SampleApp.Application.Events;
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

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId);

    private HttpRequestBuilder CreatePerson() => Http
        .CreatePerson(_body)
        .AuthenticateAs(AdminId);

    private HttpRequestBuilder GetPerson(Guid userId) => Http
        .GetPerson(userId)
        .AuthenticateAs(AdminId);

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
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNoTenantIsSpecified()
    {
        var response = await CreatePerson()
            .NoTenant()
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }
}
