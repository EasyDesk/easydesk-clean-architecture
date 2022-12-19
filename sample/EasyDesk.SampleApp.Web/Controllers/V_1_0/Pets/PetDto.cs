using EasyDesk.SampleApp.Application.Queries;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public record PetDto(int Id, string Nickname)
{
    public static PetDto FromPetSnapshot(PetSnapshot src) => new(
        Id: src.Id,
        Nickname: src.Nickname);
}
