using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Csv;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public static class PetsRoutes
{
    private const string Base = "people/{personId}/pets";

    public const string CreatePet = Base;

    public const string GetCreatePetsStatus = "bulk/" + Base;

    public const string CreatePets = "bulk/" + Base;

    public const string CreatePets2 = "bulk/" + Base + "2";

    public const string CreatePetsFromCsv = "file/" + Base;

    public const string GetOwnedPets = Base;
}

public class PetController : CleanArchitectureController
{
    [HttpPost(PetsRoutes.CreatePet)]
    public async Task<ActionResult<ResponseDto<PetDto, Nothing>>> CreatePet(
        [FromRoute] Guid personId,
        [FromBody] CreatePetBodyDto body)
    {
        return await Dispatch(new CreatePet(body.Nickname, personId))
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

    public const long MaxFileSize = 1024 * 1024 * 4;

    [HttpPost(PetsRoutes.CreatePetsFromCsv)]
    public async Task<ActionResult<ResponseDto<CreatePetsDto, Nothing>>> CreatePets(
        [FromServices] FormFileCsvParser parser,
        [FromRoute] Guid personId,
        [FromQuery] bool greedy,
        IFormFile petListCsv)
    {
        CreatePet CreatePetRowParser(CsvHelper.IReaderRow row) => new(row.GetRequiredField<string>("Nickname"), personId);
        var parsedPetList = greedy
            ? parser.GreedyParseFormFileAsCsv(formFile: petListCsv, converter: CreatePetRowParser, maxSize: MaxFileSize)
            : parser.EagerParseFormFileAsCsv(formFile: petListCsv, converter: CreatePetRowParser, maxSize: MaxFileSize);

        return await parsedPetList
            .MatchAsync(
            success: petList => Dispatch(new CreatePets(petList))
                .Map(CreatePetsDto.FromCreatePetsResult)
                .ReturnOk(),
            failure: Failure<CreatePetsDto>);
    }

    [HttpPost(PetsRoutes.CreatePets2)]
    public async Task<ActionResult<ResponseDto<CreatePetsDto, Nothing>>> CreatePets2(
        [FromRoute] Guid personId,
        [FromBody] CreatePetsBodyDto body)
    {
        return await Dispatch(new CreatePets2(body.Pets.Select(x => new CreatePet(x.Nickname, personId))))
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

    [HttpGet(PetsRoutes.GetCreatePetsStatus)]
    public async Task<ActionResult<ResponseDto<CreatePetsStatusDto, Nothing>>> GetCreatePetsStatus()
    {
        return await Dispatch(new GetBulkCreatePetsStatus())
            .MapTo<CreatePetsStatusDto>()
            .ReturnOk();
    }
}
