using EasyDesk.SampleApp.Application.V_1_0.Dto;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record CreatePersonBodyDto
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required LocalDate DateOfBirth { get; init; }

    public required AddressDto Residence { get; init; }
}
