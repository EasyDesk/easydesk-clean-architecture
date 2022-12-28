using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PetEndpoints
{
    public static HttpSingleRequestExecutor<PetDto> CreatePet(this HttpTestHelper http, Guid personId, CreatePetBodyDto body) =>
        http.Post<CreatePetBodyDto, PetDto>(PetsRoutes.CreatePet.WithRouteParam(nameof(personId), personId), body);

    public static HttpPaginatedRequestExecutor<PetDto> GetOwnedPets(this HttpTestHelper http, Guid personId) =>
        http.GetPaginated<PetDto>(PetsRoutes.GetOwnedPets.WithRouteParam(nameof(personId), personId));
}
