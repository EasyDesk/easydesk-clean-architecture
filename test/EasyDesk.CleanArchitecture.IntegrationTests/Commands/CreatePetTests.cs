using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePetTests : SampleIntegrationTest
{
    private const int BulkQuantity = 200;
    private const string TenantId = "test-tenant";
    private const string AdminId = "dog-friendly-admin";
    private const string Nickname = "Rex";

    public CreatePetTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId).AuthenticateAs(AdminId);

    [Fact]
    public async Task ShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        var response = await Http
            .CreatePet(person.Id, new(Nickname))
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    public IEnumerable<CreatePetBodyDto> PetGenerator(int count) =>
        Enumerable.Range(0, count).Select(i => new CreatePetBodyDto("buddy" + i));

    [Fact]
    public async Task BulkCreatePets_ShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() >= 1)
            .EnsureSuccess();

        var response = await Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send()
            .AsData();

        response.Pets.ShouldBe(BulkQuantity);

        var pets = await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1)
            .AsVerifiableEnumerable();

        await Verify(pets);
    }
}
