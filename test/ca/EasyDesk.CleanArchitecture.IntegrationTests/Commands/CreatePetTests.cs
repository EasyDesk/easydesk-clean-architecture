using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
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
        await Http.AddAdmin().Send().EnsureSuccess();
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            new("street", "Arthur IV", "12324", "New York", null, null, "New York State", "USA"));

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
        PetNameGenerator(count).Select(n => new CreatePetBodyDto(n));

    public IEnumerable<string> PetNameGenerator(int count) =>
        Enumerable.Range(0, count).Select(i => "buddy" + i);

    [Fact]
    public async Task BulkCreatePets_ShouldSucceed()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: new("unknown"));

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
            .Send(timeout)
            .AsData();

        response.Pets.ShouldBe(BulkQuantity);

        var pets = await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: timeout)
            .AsVerifiableEnumerable();

        await Verify(pets);
    }

    [Fact]
    public async Task BulkCreatePets_ShouldFailWithEmptyList()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            new("_"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePets(person.Id, new(PetGenerator(0)))
            .Send(timeout)
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfNotAuthorized()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: new("-"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        var response = await Http
            .CreatePet(person.Id, new(Nickname))
            .AuthenticateAs("non-admin-id")
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldSucceed()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: new("asd"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePetsFromCsv(
                person.Id,
                PetNameGenerator(BulkQuantity).ConcatStrings("\n"))
            .Send(timeout)
            .AsData();

        response.Pets.ShouldBe(BulkQuantity);

        var pets = await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: timeout)
            .AsVerifiableEnumerable();

        await Verify(pets);
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithEmptyList()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: new("__"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePetsFromCsv(person.Id, PetGenerator(0).ConcatStrings("\n"))
            .Send(timeout)
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithFileTooLarge()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            new("ooo"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePetsFromCsv(person.Id, PetGenerator(1_000_000).ConcatStrings("\n"))
            .Send(timeout)
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithInvalidFile()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            new("..."));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var response = await Http
            .CreatePetsFromCsv(person.Id, PetGenerator(2).ConcatStrings(";\n"))
            .Send(timeout)
            .AsVerifiable();

        await Verify(response);
    }
}
