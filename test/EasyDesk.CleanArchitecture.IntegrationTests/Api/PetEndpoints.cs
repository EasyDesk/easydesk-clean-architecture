using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class PetEndpoints
{
    public static HttpRequestBuilder CreatePet(this HttpTestHelper http, Guid personId, CreatePetBodyDto body) =>
        http.Post(PetsRoutes.CreatePet.WithRouteParam(nameof(personId), personId), body);

    public static HttpRequestBuilder GetOwnedPets(this HttpTestHelper http, Guid personId) =>
        http.Get(PetsRoutes.GetOwnedPets.WithRouteParam(nameof(personId), personId));
}
