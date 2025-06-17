using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public static class PersonRoutes
{
    private const string Base = "people";

    public const string CreatePerson = Base;

    public const string CreatePeople = "bulk/" + Base;

    public const string UpdatePerson = Base + "/{id}";

    public const string DeletePerson = Base + "/{id}";

    public const string GetPeople = Base;

    public const string GetPerson = Base + "/{id}";
}

public class PersonController : CleanArchitectureController
{
    [HttpPost(PersonRoutes.CreatePerson)]
    public async Task<ActionResult<ResponseDto<PersonDto, Nothing>>> CreatePerson([FromBody] CreatePersonBodyDto body)
    {
        return await Dispatch(new CreatePerson
        {
            FirstName = body.FirstName,
            LastName = body.LastName,
            DateOfBirth = body.DateOfBirth,
            Residence = body.Residence,
        })
            .ReturnCreatedAtAction(nameof(GetPerson), x => new { x.Id, });
    }

    [HttpPost(PersonRoutes.CreatePeople)]
    public async Task<ActionResult<ResponseDto<IEnumerable<PersonDto>, Nothing>>> CreatePerson([FromBody] IEnumerable<CreatePersonBodyDto> body)
    {
        return await Dispatch(new CreatePeople
        {
            People = body.Select(person => new CreatePerson
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                Residence = person.Residence,
            }),
        })
            .ReturnOk();
    }

    [HttpPut(PersonRoutes.UpdatePerson)]
    public async Task<ActionResult<ResponseDto<PersonDto, Nothing>>> UpdatePerson([FromRoute] Guid id, [FromBody] UpdatePersonBodyDto body)
    {
        return await Dispatch(new UpdatePerson(id, body.FirstName, body.LastName, body.Residence))
            .ReturnOk();
    }

    [HttpDelete(PersonRoutes.DeletePerson)]
    public async Task<ActionResult<ResponseDto<PersonDto, Nothing>>> DeletePerson([FromRoute] Guid id)
    {
        return await Dispatch(new DeletePerson(id))
            .ReturnOk();
    }

    [HttpGet(PersonRoutes.GetPeople)]
    public async Task<ActionResult<ResponseDto<IEnumerable<PersonDto>, PaginationMetaDto>>> GetPeople([FromQuery] PaginationDto pagination)
    {
        return await DispatchWithPagination(new GetPeople(), pagination)
            .ReturnOk();
    }

    [HttpGet(PersonRoutes.GetPerson)]
    public async Task<ActionResult<ResponseDto<PersonDto, Nothing>>> GetPerson([FromRoute] Guid id)
    {
        return await Dispatch(new GetPerson(id))
            .ReturnOk();
    }
}
