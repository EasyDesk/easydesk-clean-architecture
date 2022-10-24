using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Queries;

public record PersonSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    LocalDate DateOfBirth)
{
    public static PersonSnapshot FromPerson(Person person) =>
        new(person.Id, person.FirstName, person.LastName, person.DateOfBirth);
}
