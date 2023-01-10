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

    public const string CreatePets = "bulk/" + Base;

    public const string GetOwnedPets = Base;
}

public class PetController : CleanArchitectureController
{
    [HttpPost(PetsRoutes.CreatePet)]
    public async Task<ActionResult<ResponseDto<PetDto, Nothing>>> CreatePet(
        [FromRoute] Guid personId,
        [FromBody] CreatePetBodyDto body)
    {
        return await Dispatch<PetSnapshot>(new CreatePet(body.Nickname, personId))
            .Map(PetDto.FromPetSnapshot)
            .ReturnOk();
    }

    [HttpPost(PetsRoutes.CreatePets)]
    public async Task<ActionResult<ResponseDto<CreatePetsDto, Nothing>>> CreatePets(
        [FromRoute] Guid personId,
        [FromBody] CreatePetsBodyDto body)
    {
        return await Dispatch(new CreatePets(body.Pets.Select(x => new CreatePet(x.Nickname, personId))))
            .Map(CreatePetsDto.FromCreatePetsResult)
            .ReturnOk();
    }

    [HttpGet(PetsRoutes.GetOwnedPets)]
    public async Task<ActionResult<ResponseDto<IEnumerable<PetDto>, PaginationMetaDto>>> GetOwnedPets(
        [FromRoute] Guid personId,
        [FromQuery] PaginationDto pagination)
    {
        return await DispatchWithPagination(new GetOwnedPets(personId), pagination)
            .MapEachElement(PetDto.FromPetSnapshot)
            .ReturnOk();
    }
}
