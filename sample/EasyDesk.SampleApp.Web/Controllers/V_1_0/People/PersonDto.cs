using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.SampleApp.Application.Queries;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

public record PersonDto(Guid Id, string FirstName, string LastName, LocalDate DateOfBirth) : IMappableFrom<PersonSnapshot, PersonDto>
{
    public static PersonDto MapFrom(PersonSnapshot src) => new(src.Id, src.FirstName, src.LastName, src.DateOfBirth);
}
