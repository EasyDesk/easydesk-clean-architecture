using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Queries;

public record PersonSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    string CreatedBy) : IMappableFrom<Person, PersonSnapshot>
{
    public static PersonSnapshot MapFrom(Person src) =>
        new(src.Id, src.FirstName, src.LastName, src.DateOfBirth, src.CreatedBy);
}
