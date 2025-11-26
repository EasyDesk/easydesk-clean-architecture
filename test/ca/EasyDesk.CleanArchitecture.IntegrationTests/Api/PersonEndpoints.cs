using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PersonEndpoints
{
    public static HttpSingleRequestExecutor<PersonDto> CreatePerson(this HttpTestHelper http, CreatePersonBodyDto body) =>
        http.Post<CreatePersonBodyDto, PersonDto>(PersonRoutes.CreatePerson, body);

    public static HttpSingleRequestExecutor<PersonDto> UpdatePerson(this HttpTestHelper http, Guid id, UpdatePersonBodyDto body) =>
        http.Put<UpdatePersonBodyDto, PersonDto>(PersonRoutes.UpdatePerson.WithRouteParam(nameof(id), id), body);

    public static HttpSingleRequestExecutor<PersonDto> DeletePerson(this HttpTestHelper http, Guid id) =>
        http.Delete<PersonDto>(PersonRoutes.DeletePerson.WithRouteParam(nameof(id), id));

    public static HttpSingleRequestExecutor<PersonDto> GetPerson(this HttpTestHelper http, Guid id) =>
        http.Get<PersonDto>(PersonRoutes.GetPerson.WithRouteParam(nameof(id), id));

    public static HttpPaginatedRequestExecutor<IEnumerable<PersonDto>> GetPeople(this HttpTestHelper http) =>
        http.GetPaginated<IEnumerable<PersonDto>>(PersonRoutes.GetPeople);

    public static HttpSingleRequestExecutor<Option<string>> GetOptionInQuery(this HttpTestHelper http, Option<string> parameter)
    {
        return http.Get<Option<string>>(TestController.TestOptionInQueryRoute)
            .With(x =>
            {
                if (parameter.IsPresent)
                {
                    x.Query("value", parameter.Value);
                }
            });
    }

    public static HttpSingleRequestExecutor<IEnumerable<PersonDto>> CreatePeople(this HttpTestHelper http, IEnumerable<CreatePersonBodyDto> people) =>
        http.Post<IEnumerable<CreatePersonBodyDto>, IEnumerable<PersonDto>>(PersonRoutes.CreatePeople, people);
}
