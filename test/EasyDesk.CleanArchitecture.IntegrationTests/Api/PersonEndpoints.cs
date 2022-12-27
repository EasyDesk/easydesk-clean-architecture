using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PersonEndpoints
{
    public static HttpSingleRequestExecutor<PersonDto> CreatePerson(this HttpTestHelper http, CreatePersonBodyDto body) =>
        http.Post<CreatePersonBodyDto, PersonDto>(PersonRoutes.CreatePerson, body);

    public static HttpSingleRequestExecutor<PersonDto> DeletePerson(this HttpTestHelper http, Guid id) =>
        http.Delete<PersonDto>(PersonRoutes.DeletePerson.WithRouteParam(nameof(id), id));

    public static HttpSingleRequestExecutor<PersonDto> GetPerson(this HttpTestHelper http, Guid id) =>
        http.Get<PersonDto>(PersonRoutes.GetPerson.WithRouteParam(nameof(id), id));

    public static HttpPaginatedRequestExecutor<PersonDto> GetPeople(this HttpTestHelper http) =>
        http.GetPaginated<PersonDto>(PersonRoutes.GetPeople);
}
