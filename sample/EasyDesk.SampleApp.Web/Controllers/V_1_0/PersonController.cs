using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0;

public record CreatePersonBodyDto(string FirstName, string LastName, LocalDate DateOfBirth);

public record PersonDto(Guid Id, string FirstName, string LastName, LocalDate DateOfBirth)
{
    public class MappingFromSnapshot : DirectMapping<PersonSnapshot, PersonDto>
    {
        public MappingFromSnapshot() : base(src => new(src.Id, src.FirstName, src.LastName, src.DateOfBirth))
        {
        }
    }
}

public class PersonController : CleanArchitectureController
{
    [HttpPost("people")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> CreatePerson([FromBody] CreatePersonBodyDto body)
    {
        return await Dispatch(new CreatePerson.Command(body.FirstName, body.LastName, body.DateOfBirth))
            .Map(Mapper.Map<PersonDto>)
            .ReturnCreatedAtAction(nameof(GetPerson), x => new { x.Id });
    }

    [HttpDelete("people/{id}")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> DeletePerson([FromRoute] Guid id)
    {
        return await Dispatch(new DeletePerson.Command(id))
            .Map(Mapper.Map<PersonDto>)
            .ReturnOk();
    }

    [HttpGet("people")]
    public async Task<ActionResult<ResponseDto<IEnumerable<PersonDto>>>> GetPeople([FromQuery] PaginationDto pagination)
    {
        return await DispatchWithPagination(new GetPeople.Query(), pagination)
            .MapEachElement(Mapper.Map<PersonDto>)
            .ReturnOk();
    }

    [HttpGet("people/{id}")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> GetPerson([FromRoute] Guid id)
    {
        return await Dispatch(new GetPerson.Query(id))
            .Map(Mapper.Map<PersonDto>)
            .ReturnOk();
    }
}
