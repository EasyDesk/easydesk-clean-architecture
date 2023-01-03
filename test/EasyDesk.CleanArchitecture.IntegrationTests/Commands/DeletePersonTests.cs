using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Application.OutgoingEvents;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

public class DeletePersonTests : SampleIntegrationTest
{
    private const string TenantId = "test-tenant";
    private const string FirstName = "Foo";
    private const string LastName = "Bar";
    private const string AdminId = "test-admin";
    private static readonly LocalDate _dateOfBirth = new(1996, 2, 2);

    public DeletePersonTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId).AuthenticateAs(AdminId);

    private async Task<PersonDto> CreateTestPerson()
    {
        return await Http
            .Post<CreatePersonBodyDto, PersonDto>(PersonRoutes.CreatePerson, new CreatePersonBodyDto(FirstName, LastName, _dateOfBirth))
            .Send()
            .AsData();
    }

    private HttpSingleRequestExecutor<PersonDto> DeletePerson(Guid userId) => Http
        .Delete<PersonDto>(PersonRoutes.DeletePerson.WithRouteParam("id", userId));

    [Fact]
    public async Task ShouldSucceedIfThePersonExists()
    {
        var person = await CreateTestPerson();

        var response = await DeletePerson(person.Id)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfThePersonDoesNotExist()
    {
        var response = await DeletePerson(Guid.NewGuid())
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonDeleted>();

        var person = await CreateTestPerson();

        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        await bus.WaitForMessageOrFail(new PersonDeleted(person.Id));
    }

    [Fact]
    public async Task ShouldMakeItImpossibleToGetTheSamePerson()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        var response = await Http
            .GetPerson(person.Id)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMarkPersonRecordAsDeleted()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id)
            .Send()
            .EnsureSuccess();

        using var scope = WebService.Services.CreateScope();
        var personRecord = await scope.ServiceProvider
            .GetRequiredService<SampleAppContext>()
            .People
            .IgnoreQueryFilters()
            .Where(p => p.Id == person.Id)
            .FirstOptionAsync();

        personRecord.ShouldContain(p => p.IsDeleted);
    }
}
