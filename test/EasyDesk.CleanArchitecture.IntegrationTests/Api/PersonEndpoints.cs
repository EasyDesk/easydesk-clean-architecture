using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PersonEndpoints
{
    public static HttpRequestBuilder CreatePerson(this HttpTestHelper http, CreatePersonBodyDto body) =>
        http.Post(PersonRoutes.CreatePerson, body);

    public static HttpRequestBuilder DeletePerson(this HttpTestHelper http, Guid id) =>
        http.Delete(PersonRoutes.DeletePerson.WithRouteParam(nameof(id), id));

    public static HttpRequestBuilder GetPerson(this HttpTestHelper http, Guid id) =>
        http.Get(PersonRoutes.GetPerson.WithRouteParam(nameof(id), id));

    public static HttpRequestBuilder GetPeople(this HttpTestHelper http) =>
        http.Get(PersonRoutes.GetPeople);
}
