using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePetTests : SampleIntegrationTest
{
    private const string TenantId = "test-tenant";
    private const string Nickname = "Rex";

    public CreatePetTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId);

    [Fact]
    public async Task ShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12));

        var person = await Http.CreatePerson(body).AsDataOnly<PersonDto>();

        var response = await Http.CreatePet(person.Id, new(Nickname))
            .AsVerifiableResponse<PetDto>();

        var settings = new VerifySettings();
        settings.IgnoreMember<PetDto>(p => p.Id);
        await Verify(response, settings);
    }
}
