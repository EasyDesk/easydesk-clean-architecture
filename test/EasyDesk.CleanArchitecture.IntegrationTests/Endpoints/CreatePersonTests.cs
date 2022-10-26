using EasyDesk.CleanArchitecture.Web.Http;
using EasyDesk.SampleApp.Application.PropagatedEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Endpoints;

[UsesVerify]
public class CreatePersonTests : IClassFixture<SampleApplicationFactory>
{
    private const string Uri = "people";

    private readonly SampleApplicationFactory _factory;
    private readonly CleanArchitectureHttpClient _httpClient;
    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2));

    public CreatePersonTests(SampleApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateCleanArchitectureClient();
    }

    [Fact]
    public async Task CreatePersonShouldSucceed()
    {
        var response = await _httpClient
            .Post(Uri, _body)
            .As<PersonDto>();

        await Verify(new
        {
            response.HttpResponseMessage.StatusCode,
            response.Content
        });
    }

    [Fact]
    public async Task CreatePersonShouldEmitAnEvent()
    {
        await using var bus = _factory.CreateRebusHelper();
        await bus.Subscribe<PersonCreated>();

        var response = await _httpClient
            .Post(Uri, _body)
            .As<PersonDto>();

        await bus.WaitForMessageOrFail(new PersonCreated(response.Content.Data.Id));
    }
}
