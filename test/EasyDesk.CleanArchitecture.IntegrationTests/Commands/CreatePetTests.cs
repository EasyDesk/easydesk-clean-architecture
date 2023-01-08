using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.IncomingCommands;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePetTests : SampleIntegrationTest
{
    private const int BulkQuantity = 200;
    private const string Tenant = "test-tenant";
    private const string AdminId = "dog-friendly-admin";
    private const string Nickname = "Rex";

    public CreatePetTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req
        .Tenant(Tenant)
        .AuthenticateAs(AdminId);

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(Tenant));
        await WebService.WaitUntilTenantExists(TenantId.Create(Tenant));
    }

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
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send()
            .AsData();

        response.Pets.ShouldBe(BulkQuantity);

        var pets = await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: Duration.FromSeconds(15))
            .AsVerifiableEnumerable();

        await Verify(pets);
    }
}
