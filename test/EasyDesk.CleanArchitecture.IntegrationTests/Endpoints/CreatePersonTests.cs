using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Application.PropagatedEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Endpoints;

[UsesVerify]
public class CreatePersonTests : IClassFixture<SampleApplicationFactory>
{
    private const string Uri = "people";

    private readonly SampleApplicationFactory _factory;
    private readonly HttpTestHelper _httpClient;
    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2));

    public CreatePersonTests(SampleApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateHttpHelper();
    }

    [Fact]
    public async Task CreatePersonShouldSucceed()
    {
        var response = await _httpClient
            .Post(Uri, _body)
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task CreatePersonShouldEmitAnEvent()
    {
        await using var bus = _factory.CreateRebusHelper();
        await bus.Subscribe<PersonCreated>();

        var response = await _httpClient.Post(Uri, _body).AsContentOnly<PersonDto>();

        await bus.WaitForMessageOrFail(new PersonCreated(response.Data.Id));
    }
}
