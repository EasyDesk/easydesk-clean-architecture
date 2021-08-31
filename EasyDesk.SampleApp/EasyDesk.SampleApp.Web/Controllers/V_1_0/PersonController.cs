using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.SampleApp.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0
{
    public record CreatePersonBodyDto(string Name);

    public class PersonController : AbstractMediatrController
    {
        [HttpPost("people")]
        public async Task<IActionResult> CreatePerson([FromBody] CreatePersonBodyDto body)
        {
            var command = new CreatePerson.Command(body.Name);
            return await Send(command)
                .ReturnOk()
                .MapEmpty();
        }
    }
}
