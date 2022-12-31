namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;

public record CreatePetBodyDto(string Nickname);

public record CreatePetsBodyDto(IEnumerable<CreatePetBodyDto> Pets);
