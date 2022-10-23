using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0;

public record CreatePersonBodyDto(string Name);

public record PersonDto(Guid Id, string Name, bool Married)
{
    public class MappingFromSnapshot : DirectMapping<PersonSnapshot, PersonDto>
    {
        public MappingFromSnapshot() : base(src => new(src.Id, src.Name, src.Married))
        {
        }
    }
}

public class PersonController : CleanArchitectureController
{
    [HttpPost("people")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> CreatePerson([FromBody] CreatePersonBodyDto body)
    {
        return await Dispatch(new CreatePerson.Command(body.Name))
            .Map(Mapper.Map<PersonDto>)
            .ReturnCreatedAtAction(nameof(GetPerson), x => new { x.Id });
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
