using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0
{
    public record CreatePersonBodyDto(string Name);

    public record PersonDto(Guid Id, string Name, bool Married)
    {
        public class MappingFromSnapshot : DirectMapping<GetPeople.PersonSnapshot, PersonDto>
        {
            public MappingFromSnapshot() : base(src => new(src.Id, src.Name, src.Married))
            {
            }
        }
    }

    public class PersonController : AbstractMediatrController
    {
        [HttpPost("people")]
        public async Task<IActionResult> CreatePerson([FromBody] CreatePersonBodyDto body)
        {
            var command = new CreatePerson.Command(body.Name);
            return await Command(command)
                .ReturnOk()
                .MapEmpty();
        }

        [HttpGet("people")]
        public async Task<IActionResult> GetPeople([FromQuery] PaginationDto pagination)
        {
            var query = new GetPeople.Query(Mapper.Map<Pagination>(pagination));
            return await Query(query)
                .ReturnOk()
                .MapToMany<GetPeople.PersonSnapshot, PersonDto>();
        }
    }
}
