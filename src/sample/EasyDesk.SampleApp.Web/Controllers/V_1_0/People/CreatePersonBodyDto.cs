using EasyDesk.SampleApp.Application.V_1_0.Dto;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record CreatePersonBodyDto(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    AddressDto Residence);
