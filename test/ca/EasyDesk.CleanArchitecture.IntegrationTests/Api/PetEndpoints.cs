using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PetEndpoints
{
    public static HttpSingleRequestExecutor<PetDto> CreatePet(this HttpTestHelper http, Guid personId, CreatePetBodyDto body) =>
        http.Post<CreatePetBodyDto, PetDto>(PetsRoutes.CreatePet.WithRouteParam(nameof(personId), personId), body);

    public static HttpSingleRequestExecutor<CreatePetsDto> CreatePets(this HttpTestHelper http, Guid personId, CreatePetsBodyDto body) =>
        http.Post<CreatePetsBodyDto, CreatePetsDto>(PetsRoutes.CreatePets.WithRouteParam(nameof(personId), personId), body);

    public static HttpSingleRequestExecutor<CreatePetsDto> CreatePetsFromCsv(this HttpTestHelper http, Guid personId, string csv, string? fileName = null)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        using var fileContent = new StringContent(csv, new MediaTypeHeaderValue("text/csv"));
        multipartFormContent.Add(fileContent, "petListCsv", fileName ?? "pets.csv");
        return http.Request<CreatePetsDto>(
            PetsRoutes.CreatePetsFromCsv.WithRouteParam(nameof(personId), personId),
            HttpMethod.Post,
            ImmutableHttpContent.From(multipartFormContent).Result); // there is no synchronous way of serializing a MultipartFormDataContent
    }

    public static HttpPaginatedRequestExecutor<PetDto> GetOwnedPets(this HttpTestHelper http, Guid personId) =>
        http.GetPaginated<PetDto>(PetsRoutes.GetOwnedPets.WithRouteParam(nameof(personId), personId));
}
