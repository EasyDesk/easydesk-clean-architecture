using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public static class PersonRoutes
{
    private const string Base = "people";

    public const string CreatePerson = Base;

    public const string DeletePerson = Base + "/{id}";

    public const string GetPeople = Base;

    public const string GetPerson = Base + "/{id}";
}

public class PersonController : CleanArchitectureController
{
    [HttpPost(PersonRoutes.CreatePerson)]
    public async Task<ActionResult<ResponseDto<PersonDto>>> CreatePerson([FromBody] CreatePersonBodyDto body)
    {
        return await Dispatch(new CreatePerson(body.FirstName, body.LastName, body.DateOfBirth))
            .Map(PersonDto.MapFrom)
            .ReturnCreatedAtAction(nameof(GetPerson), x => new { x.Id });
    }

    [HttpDelete(PersonRoutes.DeletePerson)]
    public async Task<ActionResult<ResponseDto<PersonDto>>> DeletePerson([FromRoute] Guid id)
    {
        return await Dispatch(new DeletePerson(id))
            .Map(PersonDto.MapFrom)
            .ReturnOk();
    }

    [HttpGet(PersonRoutes.GetPeople)]
    public async Task<ActionResult<ResponseDto<IEnumerable<PersonDto>>>> GetPeople([FromQuery] PaginationDto pagination)
    {
        return await DispatchWithPagination(new GetPeople(), pagination)
            .MapEachElement(PersonDto.MapFrom)
            .ReturnOk();
    }

    [HttpGet(PersonRoutes.GetPerson)]
    public async Task<ActionResult<ResponseDto<PersonDto>>> GetPerson([FromRoute] Guid id)
    {
        return await Dispatch(new GetPerson(id))
            .Map(PersonDto.MapFrom)
            .ReturnOk();
    }
}
