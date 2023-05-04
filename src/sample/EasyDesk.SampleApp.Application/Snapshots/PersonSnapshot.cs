using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Snapshots;

public record PersonSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    string CreatedBy,
    AddressValue Residence,
    bool Approved) : ISnapshot<PersonSnapshot, Person>
{
    public static PersonSnapshot MapFrom(Person src) =>
        new(src.Id, src.FirstName, src.LastName, src.DateOfBirth, src.CreatedBy, AddressValue.MapFrom(src.Residence), src.Approved);
}
