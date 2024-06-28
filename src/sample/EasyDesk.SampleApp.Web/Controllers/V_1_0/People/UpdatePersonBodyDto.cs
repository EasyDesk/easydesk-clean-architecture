using EasyDesk.SampleApp.Application.V_1_0.Dto;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record UpdatePersonBodyDto(
    string FirstName,
    string LastName,
    AddressDto Residence);
