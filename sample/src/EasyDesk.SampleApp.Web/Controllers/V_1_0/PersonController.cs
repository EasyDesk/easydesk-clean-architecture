using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

public class PersonController : AbstractMediatrController
{
    [HttpPost("people")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> CreatePerson([FromBody] CreatePersonBodyDto body)
    {
        var response = await Send(new CreatePerson.Command(body.Name));
        return ForResponse(response)
            .Map(Mapper.Map<PersonDto>)
            .ReturnCreatedAtAction(nameof(GetPerson), x => new { x.Id });
    }

    [HttpGet("people")]
    public async Task<ActionResult<ResponseDto<IEnumerable<PersonDto>>>> GetPeople([FromQuery] PaginationDto pagination)
    {
        var response = await Send(new GetPeople.Query(pagination));
        return ForPageResponse(response)
            .MapEachElement(Mapper.Map<PersonDto>)
            .ReturnOk();
    }

    [HttpGet("people/{id}")]
    public async Task<ActionResult<ResponseDto<PersonDto>>> GetPerson([FromRoute] Guid id)
    {
        var response = await Send(new GetPerson.Query(id));
        return ForResponse(response)
            .Map(Mapper.Map<PersonDto>)
            .ReturnOk();
    }
}
