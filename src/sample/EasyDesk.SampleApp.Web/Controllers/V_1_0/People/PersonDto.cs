using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Application.Snapshots;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record PersonDto(Guid Id, string FirstName, string LastName, LocalDate DateOfBirth, string CreatedBy, AddressDto Residence) : IMappableFrom<PersonSnapshot, PersonDto>
{
    public static PersonDto MapFrom(PersonSnapshot src) => new(src.Id, src.FirstName, src.LastName, src.DateOfBirth, src.CreatedBy, AddressDto.From(src.Residence));
}
