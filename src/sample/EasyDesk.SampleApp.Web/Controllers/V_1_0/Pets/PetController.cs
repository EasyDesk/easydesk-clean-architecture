using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.FileIO;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public static class PetsRoutes
{
    private const string Base = "people/{personId}/pets";

    public const string CreatePet = Base;

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
        [FromRoute] Guid personId,
        IFormFile petListCsv)
    {
        if (petListCsv is null || petListCsv.Length <= 0)
        {
            return await Failure<CreatePetsDto>(new InputValidationError(nameof(petListCsv), "File is empty"));
        }
        if (petListCsv.Length > MaxFileSize)
        {
            return await Failure<CreatePetsDto>(new InputValidationError(nameof(petListCsv), $"File is too large (max {MaxFileSize} bytes)"));
        }
        if (petListCsv.ContentType != "text/csv" || !petListCsv.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return await Failure<CreatePetsDto>(new InputValidationError(nameof(petListCsv), $"File should be in CSV format and have text/csv as content type."));
        }
        var petList = new List<CreatePet>();
        using (var parser = new TextFieldParser(petListCsv.OpenReadStream()))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(";", ",");
            var line = 1;
            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields is not null && fields.Length == 1 && !fields[0].IsNullOrEmpty())
                {
                    petList.Add(new CreatePet(fields[0], personId));
                }
                else
                {
                    return await Failure<CreatePetsDto>(new InputValidationError(nameof(petListCsv), $"Error at line {line}. The accepted number of column is 1."));
                }
                line++;
            }
        }
        return await Dispatch(new CreatePets(petList))
            .Map(CreatePetsDto.FromCreatePetsResult)
            .ReturnOk();
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
}
