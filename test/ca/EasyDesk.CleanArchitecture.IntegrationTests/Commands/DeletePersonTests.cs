using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class DeletePersonTests : SampleIntegrationTest
{
    private const string FirstName = "Foo";
    private const string LastName = "Bar";
    private static readonly AddressDto _address = AddressDto.Create("somewhere");
    private static readonly LocalDate _dateOfBirth = new(1996, 2, 2);

    public DeletePersonTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override Option<TenantInfo> DefaultTenantInfo =>
        Some(TenantInfo.Tenant(SampleSeeder.Data.TestTenant));

    protected override async Task OnInitialization()
    {
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();
    }

    private async Task<PersonDto> CreateTestPerson()
    {
        return await Http
            .Post<CreatePersonBodyDto, PersonDto>(PersonRoutes.CreatePerson, new CreatePersonBodyDto(FirstName, LastName, _dateOfBirth, _address))
            .Send()
            .AsData();
    }

    private HttpSingleRequestExecutor<PersonDto> DeletePerson(Guid id) => Http
        .Delete<PersonDto>(PersonRoutes.DeletePerson.WithRouteParam(nameof(id), id));

    [Fact]
    public async Task ShouldSucceedIfThePersonExists()
    {
        var person = await CreateTestPerson();

        await DeletePerson(person.Id)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldFailIfThePersonDoesNotExist()
    {
        await DeletePerson(Guid.Parse("d9dac153-39d9-4128-89db-fc854ac4b96e"))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldNotEmitAnEvent_IfFailed()
    {
        await DefaultBusEndpoint.Subscribe<PersonDeleted>();

        await DeletePerson(Guid.Parse("d9dac153-39d9-4128-89db-fc854ac4b96e"))
            .Send()
            .EnsureFailure();

        await DefaultBusEndpoint.FailIfMessageIsReceived<PersonDeleted>();
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await DefaultBusEndpoint.Subscribe<PersonDeleted>();

        var person = await CreateTestPerson();

        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        await DefaultBusEndpoint.WaitForMessageOrFail(new PersonDeleted(person.Id));
    }

    [Fact]
    public async Task ShouldMakeItImpossibleToGetTheSamePerson()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        await Http
            .GetPerson(person.Id)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMarkPersonRecordAsDeleted()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        await using var scope = WebService.Services.CreateAsyncScope();
        var personRecord = await scope.ServiceProvider
            .GetRequiredService<SampleAppContext>()
            .People
            .IgnoreQueryFilters()
            .Where(p => p.Id == person.Id)
            .FirstOptionAsync();

        personRecord.IsAbsent.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldFailIfNotAuthorized()
    {
        var person = await CreateTestPerson();

        await DeletePerson(person.Id)
            .AuthenticateAs(TestAgents.OtherUser)
            .Send()
            .Verify();
    }
}
