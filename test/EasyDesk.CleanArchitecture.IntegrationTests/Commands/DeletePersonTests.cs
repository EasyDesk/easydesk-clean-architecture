using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
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
    private const string FirstName = "Foo";
    private const string LastName = "Bar";

    private static readonly LocalDate _dateOfBirth = new(1996, 2, 2);

    public DeletePersonTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    private async Task<PersonDto> CreateTestPerson()
    {
        return await Http
            .Post(PersonRoutes.CreatePerson, new CreatePersonBodyDto(FirstName, LastName, _dateOfBirth))
            .AsDataOnly<PersonDto>();
    }

    private HttpRequestBuilder DeletePerson(Guid id) =>
        Http.Delete(PersonRoutes.DeletePerson.WithRouteParam("id", id));

    [Fact]
    public async Task DeletePersonShouldSucceedIfThePersonExists()
    {
        var person = await CreateTestPerson();

        var response = await DeletePerson(person.Id)
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task DeletePersonShouldFailIfThePersonDoesNotExist()
    {
        var response = await DeletePerson(Guid.NewGuid())
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task DeletePersonShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonDeleted>();

        var person = await CreateTestPerson();

        await DeletePerson(person.Id).IgnoringResponse();

        await bus.WaitForMessageOrFail(new PersonDeleted(person.Id));
    }

    [Fact]
    public async Task DeletePersonShouldMakeItImpossibleToGetTheSamePerson()
    {
        var person = await CreateTestPerson();
        await DeletePerson(person.Id).IgnoringResponse();

        var response = await Http
            .Get(PersonRoutes.GetPerson.WithRouteParam("id", person.Id))
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task DeletePersonShouldMarkPersonRecordAsDeleted()
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
