using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PetEndpoints
{
    public static HttpSingleRequestExecutor<PetDto> CreatePet(this HttpTestHelper http, Guid personId, PetInfoDto body) =>
        http.Post<PetInfoDto, PetDto>(PetsRoutes.CreatePet.WithRouteParam(nameof(personId), personId), body);

    public static HttpSingleRequestExecutor<CreatePetsResultDto> CreatePets(this HttpTestHelper http, Guid personId, CreatePetsBodyDto body) =>
        http.Post<CreatePetsBodyDto, CreatePetsResultDto>(PetsRoutes.CreatePets.WithRouteParam(nameof(personId), personId), body);

    public static HttpSingleRequestExecutor<CreatePetsResultDto> CreatePets2(this HttpTestHelper http, Guid personId, CreatePetsBodyDto body) =>
        http.Post<CreatePetsBodyDto, CreatePetsResultDto>(PetsRoutes.CreatePets2.WithRouteParam(nameof(personId), personId), body);

    public static HttpSingleRequestExecutor<CreatePetsStatusDto> GetCreatePetsStatus(this HttpTestHelper http) =>
        http.Get<CreatePetsStatusDto>(PetsRoutes.GetCreatePetsStatus);

    public static HttpSingleRequestExecutor<CreatePetsResultDto> CreatePetsFromCsv(this HttpTestHelper http, Guid personId, string csv, string? fileName = null)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        using var fileContent = new StringContent(csv, new MediaTypeHeaderValue("text/csv"));
        multipartFormContent.Add(fileContent, "petListCsv", fileName ?? "pets.csv");
        return http.Request<CreatePetsResultDto>(
            PetsRoutes.CreatePetsFromCsv.WithRouteParam(nameof(personId), personId),
            HttpMethod.Post,
            ImmutableHttpContent.From(multipartFormContent).Result); // there is no synchronous way of serializing a MultipartFormDataContent
    }

    public static HttpPaginatedRequestExecutor<PetDto> GetOwnedPets(this HttpTestHelper http, Guid personId) =>
        http.GetPaginated<PetDto>(PetsRoutes.GetOwnedPets.WithRouteParam(nameof(personId), personId));
}
