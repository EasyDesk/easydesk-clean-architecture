using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Web.Http;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Endpoints;

[UsesVerify]
public class CreatePersonTests : IClassFixture<SampleApplicationFactory>
{
    private readonly CleanArchitectureHttpClient _httpClient;

    public CreatePersonTests(SampleApplicationFactory factory)
    {
        _httpClient = new CleanArchitectureHttpClient(
            factory.CreateClient(),
            factory.Services.GetRequiredService<JsonSettingsConfigurator>());
    }

    [Fact]
    public async Task CreatePersonShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1996, 2, 2));

        var response = await _httpClient
            .Post("people", body)
            .As<PersonDto>();

        await Verify(new
        {
            response.HttpResponseMessage.StatusCode,
            response.Content
        });
    }
}
