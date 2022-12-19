using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public static class PetsRoutes
{
    private const string Base = "people/{personId}/pets";

    public const string CreatePet = Base;

    public const string GetOwnedPets = Base;
}

public class PetController : CleanArchitectureController
{
    [HttpPost(PetsRoutes.CreatePet)]
    public async Task<ActionResult<ResponseDto<PetDto>>> CreatePet(
        [FromRoute] Guid personId,
        [FromBody] CreatePetBodyDto body)
    {
        return await Dispatch(new CreatePet(body.Nickname, personId))
            .Map(PetDto.FromPetSnapshot)
            .ReturnOk();
    }

    [HttpGet(PetsRoutes.GetOwnedPets)]
    public async Task<ActionResult<ResponseDto<IEnumerable<PetDto>>>> GetPets(
        [FromRoute] Guid personId,
        [FromQuery] PaginationDto pagination)
    {
        return await DispatchWithPagination(new GetOwnedPets(personId), pagination)
            .MapEachElement(PetDto.FromPetSnapshot)
            .ReturnOk();
    }
}
