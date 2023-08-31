using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class CreatePetTests : SampleIntegrationTest
{
    private const int BulkQuantity = 200;
    private const string Nickname = "Rex";
    private static readonly TenantId _tenant = TenantId.New("test-tenant");

    public CreatePetTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task OnInitialization()
    {
        await DefaultBusEndpoint.Send(new CreateTenant(_tenant));
        await WebService.WaitUntilTenantExists(_tenant);

        TenantNavigator.MoveToTenant(_tenant);
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            new(
                StreetType: Some("street"),
                StreetName: "Arthur IV",
                StreetNumber: Some("12324"),
                City: Some("New York"),
                District: None,
                Province: None,
                Region: Some("New York State"),
                State: Some("USA"),
                Country: None));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .CreatePet(person.Id, new(Nickname))
            .Send()
            .Verify();
    }

    public IEnumerable<PetInfoDto> PetGenerator(long count) =>
        PetNameGenerator(count).Select(n => new PetInfoDto(n));

    public IEnumerable<string> PetNameGenerator(long count)
    {
        for (var i = 0L; i < count; i++)
        {
            yield return "buddy" + i;
        }
    }

    public string GenerateCsv(long count) =>
        PetNameGenerator(count).Prepend("Nickname").ConcatStrings("\n");

    [Fact]
    public async Task BulkCreatePets_ShouldSucceed()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("unknown"));

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

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePets_ShouldFailWithEmptyList()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            AddressDto.Create("_"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Http
            .CreatePets(person.Id, new(PetGenerator(0)))
            .Send(timeout)
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfNotAuthorized()
    {
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("-"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .CreatePet(person.Id, new(Nickname))
            .AuthenticateAs(TestAgents.OtherUser)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldSucceed()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("asd"));

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
                GenerateCsv(BulkQuantity))
            .Send(timeout)
            .AsData();

        response.Pets.ShouldBe(BulkQuantity);

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithEmptyList()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("__"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Http
            .CreatePetsFromCsv(person.Id, GenerateCsv(0))
            .Send(timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithFileTooLarge()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            AddressDto.Create("ooo"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Http
            .CreatePetsFromCsv(person.Id, GenerateCsv(PetController.MaxFileSize / 4))
            .Send(timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithInvalidFile_WithEagerParsing()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            AddressDto.Create("..."));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Http
            .CreatePetsFromCsv(person.Id, GenerateCsv(20).Replace("Nickname", "Nick;Name"))
            .Send(timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePetsFromCsv_ShouldFailWithInvalidFile_WithGreedyParsing()
    {
        var timeout = Duration.FromSeconds(15);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            AddressDto.Create("..."));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        await Http
            .CreatePetsFromCsv(person.Id, GenerateCsv(20).Replace("Nickname", "Nick;Name"))
            .WithQuery("greedy", true.ToString())
            .Send(timeout)
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePets_ShouldFailInParallel()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("unknown"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        Task<VerifiableHttpResponse<CreatePetsResultDto, Nothing>> StartBulkOperation() => Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send(timeout)
            .AsVerifiable();

        var success = await StartBulkOperation();

        var failure = await StartBulkOperation();

        await Verify(new { Success = success, Failure = failure });
    }

    [Fact]
    public async Task BulkCreatePets_ShouldSucceedInParallel_WithDifferentOperations()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("unknown"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Any())
            .EnsureSuccess();

        var success = await Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send(timeout)
            .AsVerifiable();

        var successToo = await Http
            .CreatePets2(person.Id, new(PetGenerator(BulkQuantity)))
            .Send(timeout)
            .AsVerifiable();

        await Verify(new { Success = success, SuccessToo = successToo });
    }

    [Fact]
    public async Task BulkCreatePets_ShouldNotBeInProgressByDefault()
    {
        await Http.GetCreatePetsStatus().Send().Verify();
    }

    [Fact]
    public async Task BulkCreatePets_ShouldBeInProgress_AfterStartingBulkOperation()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("unknown"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send(timeout)
            .EnsureSuccess();

        await Http
            .GetCreatePetsStatus()
            .Send()
            .Verify();
    }

    [Fact]
    public async Task BulkCreatePets_ShouldNotBeInProgress_AfterCompletion()
    {
        var timeout = Duration.FromSeconds(30);
        var body = new CreatePersonBodyDto(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12),
            Residence: AddressDto.Create("unknown"));

        var person = await Http
            .CreatePerson(body)
            .Send()
            .AsData();

        await Http
            .CreatePets(person.Id, new(PetGenerator(BulkQuantity)))
            .Send(timeout)
            .EnsureSuccess();

        await Http
            .GetOwnedPets(person.Id)
            .PollUntil(pets => pets.Count() == BulkQuantity + 1, timeout: timeout)
            .EnsureSuccess();

        await Http
            .GetCreatePetsStatus()
            .Send()
            .Verify();
    }
}
