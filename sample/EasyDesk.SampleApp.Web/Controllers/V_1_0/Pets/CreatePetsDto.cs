using EasyDesk.SampleApp.Application.Commands;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public record CreatePetsDto(int Pets)
{
    public static CreatePetsDto FromCreatePetsResult(CreatePetsResult result) => new(result.Pets);
}
