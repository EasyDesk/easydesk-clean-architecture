using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.SampleApp.Application.Events;
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

    public DeletePersonTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) => req.Tenant(TenantId);

    private async Task<PersonDto> CreateTestPerson()
    {
        return await Http
            .Post(PersonRoutes.CreatePerson, new CreatePersonBodyDto(FirstName, LastName, _dateOfBirth))
            .AuthenticateAs(AdminId)
            .AsDataOnly<PersonDto>();
    }

    private HttpRequestBuilder DeletePerson(Guid userId) => Http
        .Delete(PersonRoutes.DeletePerson.WithRouteParam("id", userId))
        .AuthenticateAs(AdminId);

    [Fact]
    public async Task ShouldSucceedIfThePersonExists()
    {
        var person = await CreateTestPerson();

        var response = await DeletePerson(person.Id)
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldFailIfThePersonDoesNotExist()
    {
        var response = await DeletePerson(Guid.NewGuid())
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonDeleted>();

        var person = await CreateTestPerson();

        await DeletePerson(person.Id).IgnoringResponse();

        await bus.WaitForMessageOrFail(new PersonDeleted(person.Id));
    }

    [Fact]
    public async Task ShouldMakeItImpossibleToGetTheSamePerson()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id).IgnoringResponse();

        var response = await Http.Get(PersonRoutes.GetPerson.WithRouteParam("id", person.Id))
            .AuthenticateAs(AdminId)
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMarkPersonRecordAsDeleted()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id).IgnoringResponse();

        using var scope = Factory.Services.CreateScope();
        var personRecord = await scope.ServiceProvider
            .GetRequiredService<SampleAppContext>()
            .People
            .IgnoreQueryFilters()
            .Where(p => p.Id == person.Id)
            .FirstOptionAsync();

        personRecord.ShouldContain(p => p.IsDeleted);
    }
}
